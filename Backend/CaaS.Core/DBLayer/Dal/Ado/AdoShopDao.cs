using CaaS.Core.Dal.Common;
using CaaS.Core.Dal.Interface;
using CaaS.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Dal.Ado
{

    public class AdoShopDao: IShopDao
    {
        private WeekDayDistribution<decimal> MapRowToWeekdaDistribution(IDataRecord row)
        {
            return new(
                DBNull.Value.Equals(row["sunday"])? 0M : (decimal)row["sunday"],
                DBNull.Value.Equals(row["monday"])? 0M : (decimal)row["monday"],
                DBNull.Value.Equals(row["tuesday"])? 0M : (decimal)row["tuesday"],
                DBNull.Value.Equals(row["wednesday"])? 0M : (decimal)row["wednesday"],
                DBNull.Value.Equals(row["thursday"])? 0M : (decimal)row["thursday"],
                DBNull.Value.Equals(row["friday"])? 0M : (decimal)row["friday"],
                DBNull.Value.Equals(row["saturday"])? 0M : (decimal)row["saturday"]
            );
        }

        private ProductAmount MapRowToProductAmount(IDataRecord row)
        {
            return new(
                new(
                    (Guid)row["id"],
                    (string)row["name"],
                    (decimal)row["price"],
                    (string)row["description"],
                    (int)row["imagenr"],
                    (string)row["downloadlink"]
                ),
                (int)row["count"]
            );
        }

        private Shop MapRowToShop(IDataRecord row)
        {
            return new(
                (Guid)row[nameof(Shop.Id)],
                (string)row[nameof(Shop.Name)],
                (string)row[nameof(Shop.AppKey)]
            );
        }

        private readonly AdoTemplate template;

        public AdoShopDao(IConnectionFactory connectionFactory)
        {
            template = new AdoTemplate(connectionFactory);
        }
        public async Task<IEnumerable<Shop>> FindAllAsync()
        {
            return await template.QueryAsync(
                "select * from shop",
                MapRowToShop
            );
        }
        public async Task<Shop?> FindByIdAsync(Guid id)
        {
            return (await template.QueryAsync(
                "select * from shop where id = @id",
                MapRowToShop,
                new QueryParameter("@id", id)
            )).FirstOrDefault();
        }
        public async Task UpdateAsync(Shop shop)
        {
            await template.ExecuteAsync(
                "update shop " +
                "set name = @n, appkey = @ak " +
                "where id = @id",
                new QueryParameter("@n", shop.Name),
                new QueryParameter("@ak", shop.AppKey),
                new QueryParameter("@id", shop.Id)
            );
        }
        public async Task InsertAsync(Shop shop)
        {
            var id = await template.ExecuteScalarAsync<Guid>(
                "declare @guid uniqueidentifier = newid();" +
                "insert into shop " +
                "(id, name, appkey) " +
                "values (@guid, @n, @ak); " +
                "select @guid",
                new QueryParameter("@n", shop.Name),
                new QueryParameter("@ak", shop.AppKey)
            );
            shop.Id = id;
        }

        /// <summary>
        /// gets the top 'count' products ordered by how often it has been saled
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="count"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ProductAmount>> EvaluateTopsellersAsync(Guid shopId, int count, DateTime start, DateTime end)
        {
            return await template.QueryAsync(
                "select top (@cnt) product.id as id, name, price, description, downloadlink, imagenr, count " +
                "from product inner join ( " +
                    "select product.id as id, sum(count) as count " +
                    "from product " +
                    "inner join orderproduct on product.id = orderproduct.originalid " +
                    "inner join orderentry on orderentry.orderid = orderproduct.orderid " +
                        "and orderentry.rownr = orderproduct.rownr " +
                    "inner join [order] on orderentry.orderid = [order].id " +
                    "where product.shopid = @sid and product.price > 0 " +
                        "and [order].datetime >= @beg " +
                        "and [order].datetime <= @end " +
                    "group by product.id " +
                ") sel on product.id = sel.id " +
                "order by count desc",
                MapRowToProductAmount,
                new QueryParameter("@cnt", count),
                new QueryParameter("@sid", shopId),
                new QueryParameter("@beg", start),
                new QueryParameter("@end", end)
            );
        }

        /// <summary>
        /// evaluates the total shop sales in the given range
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>

        public async Task<decimal> EvaluateSalesAsync(Guid shopId, DateTime start, DateTime end)
        {
            return await template.ExecuteScalarAsync<decimal>(
                "select coalesce(sum(ordersum), 0) from (" +
                    "select [order].id, (sum(price * count) - offsum) as ordersum " +
                    "from [order] " +
                    "inner join orderentry on [order].id = orderentry.orderid " +
                    "inner join orderproduct on orderentry.orderid = orderproduct.orderid " +
                        "and orderentry.rownr = orderproduct.rownr " +
                    "where [order].shopid = @sid " +
                        "and [order].datetime >= @beg " +
                        "and [order].datetime <= @end " +
                    "group by [order].id, offsum " +
                ") sel",
                new QueryParameter("@sid", shopId),
                new QueryParameter("@beg", start),
                new QueryParameter("@end", end)
            );
        }
        /// <summary>
        /// evaluates the sum of all orders in the given range per weekday
        /// </summary>
        /// <param name="id"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<WeekDayDistribution<decimal>> EvaluateCartSalesDistributed(Guid id, DateTime start, DateTime end)
        {
            return (await template.QueryAsync(
                "select * " +
                "from ( " +
                    "select datename(weekday, datetime) as weekday, sum(price * count) - offsum as cost " +
                    "from [order] " +
                        "inner join orderentry on [order].id = orderentry.orderid " +
                        "inner join orderproduct on orderentry.orderid = orderproduct.orderid " +
                            "and orderentry.rownr = orderproduct.rownr " +
                    "where shopid = @sid and " +
                    "datetime >= @start and datetime <= @end " +
                    "group by [order].id, datetime, offsum " +
                ") as sourcetable " +
                "pivot ( " +
                    "sum(cost) for weekday in ( " +
                        "[sunday], [monday], [tuesday], [wednesday], " +
                        "[thursday], [friday], [saturday] " +
                    ") " +
                ") as pivottable",
                MapRowToWeekdaDistribution,
                new QueryParameter("@sid", id),
                new QueryParameter("@start", start),
                new QueryParameter("@end", end)
            )).SingleOrDefault() ?? new WeekDayDistribution<decimal>(0, 0, 0, 0, 0, 0, 0);
        }

        /// <summary>
        /// evaluates average product amount in orders per weekday in the given interval.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>

        public async Task<WeekDayDistribution<decimal>> EvaluateCartProductCountDistributed(Guid id, DateTime start, DateTime end)
        {
            return (await template.QueryAsync(
                "select * " +
                "from ( " +
                    "select datename(weekday, datetime) as weekday, cast(count as decimal(12, 6)) as count " +
                    "from [order] " +
                        "inner join orderentry on [order].id = orderentry.orderid " +
                        "inner join orderproduct on orderentry.orderid = orderproduct.orderid " +
                            "and orderentry.rownr = orderproduct.rownr " +
                    "where orderproduct.price > 0 and shopid = @sid " +
                        "and datetime >= @start and datetime <= @end " +
                ") as sourcetable " +
                "pivot ( " +
                    "avg(count) for weekday in ( " +
                        "[sunday], [monday], [tuesday], [wednesday], " +
                        "[thursday], [friday], [saturday] " +
                    ") " +
                ") as pivottable",
                MapRowToWeekdaDistribution,
                new QueryParameter("@sid", id),
                new QueryParameter("@start", start),
                new QueryParameter("@end", end))
            ).SingleOrDefault()?? new WeekDayDistribution<decimal>(0,0,0,0,0,0,0);
        }

        public async Task DeleteAsync(Guid id)
        {
            await template.ExecuteAsync(
                "delete from shop where id = @sid",
                new QueryParameter("@sid", id)
            ); 
        }
    }
}
