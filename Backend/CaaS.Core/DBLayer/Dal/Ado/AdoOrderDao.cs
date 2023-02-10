using CaaS.Core.Dal.Common;
using CaaS.Core.Dal.Interface;
using CaaS.Core.Domain;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace CaaS.Core.Dal.Ado
{
    public class AdoOrderDao : IOrderDao
    {
        private readonly AdoTemplate template;

        public AdoOrderDao(IConnectionFactory connectionFactory)
        {
            template = new AdoTemplate(connectionFactory);
        }

        #region helper methods

        private static (Order Order, OrderEntry OrderEntry) MapToOrderOrderEntry(IDataRecord row) {
            return new(
                new Order(
                    (Guid)row["orderid"],
                    (DateTime)row["datetime"],
                    (decimal)row["offsum"],
                    new Customer(
                        (string)row["firstname"],
                        (string)row["lastname"],
                        (string)row["email"]
                    ),
                    Enumerable.Empty<OrderEntry>(),
                    (decimal)row["total"],
                    (string)row["downloadlink"]
                ),
                new OrderEntry(
                    (int)row["rownr"],
                    (int)row["count"],
                    new OrderProduct(
                        (Guid)row["originalid"],
                        (string)row["name"],
                        (decimal)row["price"]
                    )
                )
            );
        }

        #endregion

        public async Task<Order?> FindByIdAsync(Guid shopId, Guid id)
        {
            return (await template.QueryAsync(
                "select [order].id as orderid, datetime, offsum, " +
                    "orderentry.rownr as rownr, count, originalid, name, price, " +
                    "firstname, lastname, email, total, downloadlink " +
                    "from [order] " +
                "inner join customer on [order].id = customer.orderid " +
                "inner join orderentry on [order].id = orderentry.orderid " +
                "inner join orderproduct on orderentry.orderid = orderproduct.orderid " +
                    "and orderentry.rownr = orderproduct.rownr " +
                "where [order].id = @id and [order].shopid = @sid",
                MapToOrderOrderEntry,
                new QueryParameter("@id", id),
                new QueryParameter("@sid", shopId)
            )).GroupBy(grp => grp.Order, grp => grp.OrderEntry).Select(x => new Order(
                x.Key.Id, x.Key.DateTime, x.Key.OffSum, x.Key.Customer, x, x.Key.DownloadLink
            )).FirstOrDefault();
        }

        public async Task InsertAsync(Guid shopId, Order order)
        {
            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
                order.Id = await template.ExecuteScalarAsync<Guid>(
                    "declare @guid uniqueidentifier = newid();" +
                    "insert into [order] (id, datetime, offsum, shopid, total, downloadlink) " +
                    "values (@guid, @dt, @os, @sid, @total, @dload); " +
                    "select @guid",
                    new QueryParameter("@dt", order.DateTime),
                    new QueryParameter("@os", order.OffSum),
                    new QueryParameter("@sid", shopId),
                    new QueryParameter("@total", order.Total),
                    new QueryParameter("@dload", order.DownloadLink)
                );

                await template.ExecuteAsync(
                    "insert into customer (orderid, lastname, firstname, email) " +
                    "values (@oid, @ln, @fn, @em)",
                    new QueryParameter("@oid", order.Id),
                    new QueryParameter("@ln", order.Customer.Lastname),
                    new QueryParameter("@fn", order.Customer.Firstname),
                    new QueryParameter("@em", order.Customer.Email)
                );


                var entryParams = new List<QueryParameter>();
                var productParams = new List<QueryParameter>();
                var sbEntry = new StringBuilder(
                    "insert into orderentry (rownr, orderid, count) values "
                );
                var sbProduct = new StringBuilder(
                    "insert into orderproduct (rownr, orderid, price, originalid, name) values "
                );

                int i = 0;
                foreach (var entry in order.Entries!)
                {
                    sbEntry.Append($"(@rn{i}, @oid{i}, @cnt{i}),");
                    sbProduct.Append($"(@rn{i}, @oid{i}, @pr{i}, @orig{i}, @nm{i}),");

                    entryParams.Add(new QueryParameter($"@rn{i}", entry.RowNumber));
                    entryParams.Add(new QueryParameter($"@oid{i}", order.Id));
                    entryParams.Add(new QueryParameter($"@cnt{i}", entry.Count));

                    productParams.Add(new QueryParameter($"@rn{i}", entry.RowNumber));
                    productParams.Add(new QueryParameter($"@oid{i}", order.Id));
                    productParams.Add(new QueryParameter($"@pr{i}", entry.Product!.Price));
                    productParams.Add(new QueryParameter($"@orig{i}", entry.Product!.OriginalId));
                    productParams.Add(new QueryParameter($"@nm{i++}", entry.Product!.Name));
                }

                sbEntry.Remove(sbEntry.Length - 1, 1);
                sbProduct.Remove(sbProduct.Length - 1, 1);


                await template.ExecuteAsync(
                    sbEntry.ToString(),
                    entryParams.ToArray()
                );

                await template.ExecuteAsync(
                    sbProduct.ToString(),
                    productParams.ToArray()
                );

                transactionScope.Complete();
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Order>> FindAllForShopAsync(Guid shopId, DateTime from, DateTime to)
        {
            return (await template.QueryAsync(
                "select [order].id as orderid, datetime, offsum, " +
                    "orderentry.rownr as rownr, count, originalid, name, price, " +
                    "firstname, lastname, email, total, downloadlink " +
                    "from [order] " +
                    "inner join customer on [order].id = customer.orderid " +
                    "inner join orderentry on [order].id = orderentry.orderid " +
                    "inner join orderproduct on orderentry.orderid = orderproduct.orderid " +
                        "and orderentry.rownr = orderproduct.rownr " +
                "where [order].shopid = @sid and datetime >= @from and datetime <= @to",
                MapToOrderOrderEntry,
                new QueryParameter("@sid", shopId),
                new QueryParameter("@from", from),
                new QueryParameter("@to", to)
            )).GroupBy(grp => grp.Order, grp => grp.OrderEntry).OrderByDescending(x => x.Key.DateTime).Select(x => new Order(
                x.Key.Id, x.Key.DateTime, x.Key.OffSum, x.Key.Customer, x, x.Key.DownloadLink
            ));
        }

        private class OrderCompareElem : IComparable
        {
            public OrderCompareElem(decimal value, DateTime dateTime)
            {
                Value = value;
                DateTime = dateTime;
            }

            public decimal Value { get; set; }
            public DateTime DateTime { get; set; }

            public int CompareTo(object? obj)
            {
                if(obj is not OrderCompareElem other)
                {
                    throw new ArgumentException($"compared object must be of type '{nameof(OrderCompareElem)}'");
                }
                else
                {
                    if (Value > other.Value) return -1;
                    if (other.Value > Value) return 1;
                    if (DateTime < other.DateTime) return -1;
                    if (other.DateTime < DateTime) return 1;
                    return 0;
                }
            }
        }

        public async Task<IEnumerable<Order>> SearchOrders(Guid shopId, DateTime from, DateTime to, string searchTerm)
        {
            return (await template.QueryAsync(
                "declare @search as varchar(50) = @term;" +
                "select oc.id as orderid, datetime, offsum, oe.rownr as rownr, count, total, downloadlink, " +
                    "originalid, name, price, firstname, lastname, email, value from (" +
                    "select *, (" +
                        "case when convert(nvarchar(36) ,id) = @search then 100 when charindex(@search, o.id) > 0 then 8 else  0 end + " +
                        "case when @search = lastname then 12 else difference(lastname, @search) end + " +
                        "case when @search = firstname then 8 else difference(firstname, @search) end " +
                    ") as value " +
                    "from [order] o " +
                    "inner join customer c on c.orderid = o.id " +
                    "where datetime >= @start and datetime <= @end " +
                        "and shopid = @sid " +
                ") oc " +
                "inner join orderentry oe on oc.id = oe.orderid " +
                "inner join orderproduct op on oc.id = op.orderid and op.rownr = oe.rownr " +
                "where value > 4 ",
                row =>
                    new
                    {
                        Ooe = MapToOrderOrderEntry(row),
                        Value = (int)row["value"]
                    }
                ,
                new QueryParameter("@term", searchTerm),
                new QueryParameter("@start", from),
                new QueryParameter("@end", to),
                new QueryParameter("@sid", shopId)
            )).GroupBy(grp => new { grp.Ooe.Order, grp.Value }, grp => grp.Ooe.OrderEntry)
            .OrderBy(ooe => new OrderCompareElem(ooe.Key.Value, ooe.Key.Order.DateTime))
            .Select(x => new Order(
                x.Key.Order.Id, x.Key.Order.DateTime, x.Key.Order.OffSum, x.Key.Order.Customer, x.OrderBy(oe => oe.RowNumber), x.Key.Order.DownloadLink
            ));
        }
    }
}
