using CaaS.Core.Dal.Common;
using CaaS.Core.Dal.Interface;
using CaaS.Core.DBLayer.Domain;
using CaaS.Core.Domain;
using MySqlX.XDevAPI.Relational;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Transactions;

namespace CaaS.Core.Dal.Ado
{
    public class AdoDiscountDao : IDiscountDao
    {
        private readonly AdoTemplate template;

        public AdoDiscountDao(IConnectionFactory connectionFactory)
        {
            template = new AdoTemplate(connectionFactory);
        }

        public static Discount MapRowToDiscount(IDataRecord row)
        {
            return new(
                (Guid)row["id"],
                (OffType)(int)row["offtype"],
                (decimal)row["offvalue"],
                (string)row["description"],
                (string)row["tag"],
                (MinType)(int)row["mintype"],
                (decimal)row["minvalue"],
                (bool)row["is4allproducts"],
                (DateTime)row["validfrom"],
                (DateTime)row["validto"]
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

        private static (Discount Discount, ProductAmount? ProductAmount, Guid Pid) MapRowToDiscountFreeProduct(IDataRecord row)
        {
            return new(
                MapRowToDiscount(row),
                DBNull.Value.Equals(row["productid"]) ? null :
                new (
                    new Product(
                       (Guid)row["productid"],
                       (string)row["name"],
                       (decimal)row["price"],
                       (string)row["productdescription"],
                       (int)row["imagenr"],
                       (string)row["downloadlink"]
                    ), (int)row["count"]
                ),
                DBNull.Value.Equals(row["pid"]) ? Guid.Empty :
                (Guid)row["pid"]
            );
        }

        private class CartentryDiscount
        {
            public int Key;
            public Discount Discount;
            public ProductAmount? ProductAmount;

            public CartentryDiscount(int key, Discount discount, ProductAmount? productAmount = null)
            {
                Key = key;
                Discount = discount;
                ProductAmount = productAmount;
            }
        }

        public async Task InsertAsync(Guid shopid, Discount discount)
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var id = await template.ExecuteScalarAsync<Guid>(
                    "declare @guid uniqueidentifier = newid();" +
                    "insert into discount (id, shopid, offtype, offvalue, description, tag, " +
                    "mintype, minvalue, validfrom, validto, is4allproducts) " +
                    "values (@guid, @sid, @ot, @ov, @des, @tag, @mt, @mv, @vf, @vt, @i4p); " +
                    "select @guid",
                    new QueryParameter("@sid", shopid),
                    new QueryParameter("@ot", discount.OffType),
                    new QueryParameter("@ov", discount.OffValue),
                    new QueryParameter("@des", discount.Description),
                    new QueryParameter("@tag", discount.Tag),
                    new QueryParameter("@mt", discount.MinType),
                    new QueryParameter("@mv", discount.MinValue),
                    new QueryParameter("@vf", discount.ValidFrom),
                    new QueryParameter("@vt", discount.ValidTo),
                    new QueryParameter("@i4p", discount.Is4AllProducts)
                );

                int i = 0;
                if (discount.Products.Any())
                {
                    var sb = new StringBuilder(
                        "insert into productdiscountproduct (discountid, productid) " +
                        "values "
                    );

                    var queryParams = new List<QueryParameter>();
                    
                    foreach (var pid in discount.Products)
                    {
                        sb.Append($"(@did{i}, @pid{i}),");
                        queryParams.Add(new QueryParameter($"@did{i}", id));
                        queryParams.Add(new QueryParameter($"@pid{i++}", pid));
                    }

                    sb.Remove(sb.Length - 1, 1);

                    await template.ExecuteAsync(
                        sb.ToString(), queryParams.ToArray()
                    );
                }

                if (discount.FreeProducts.Any())
                {
                    var sb = new StringBuilder(
                        "insert into discountfreeproduct (discountid, productid, count) " +
                        "values "
                    );

                    var queryParams = new List<QueryParameter>();
                    foreach (var tuple in discount.FreeProducts)
                    {
                        sb.Append($"(@did{i}, @pid{i}, @cnt{i}),");
                        queryParams.Add(new QueryParameter($"@did{i}", id));
                        queryParams.Add(new QueryParameter($"@pid{i}", tuple.Product.Id));
                        queryParams.Add(new QueryParameter($"@cnt{i++}", tuple.Count));
                    }

                    sb.Remove(sb.Length - 1, 1);

                    await template.ExecuteAsync(
                        sb.ToString(), queryParams.ToArray()
                    );
                }

                scope.Complete();
                discount.Id = id;
            }
            catch
            {
                throw;
            }               
        }

        public async Task<IEnumerable<Discount>> FindAllAsync(Guid shopId)
        {
            return (await template.QueryAsync(
                "select discount.id as id, offtype, offvalue, validfrom, validto, " +
                    "discount.description as description, tag, mintype, minvalue, is4allproducts, " +
                    "product.id as productid, product.name as name, product.price as price, " +
                    "product.description as productdescription, product.downloadlink as downloadlink, product.imagenr as imagenr, " +
                    "count, null as pid " +
                "from discount " +
                "left outer join discountfreeproduct " +
                    "on discount.id = discountfreeproduct.discountid " +
                "left outer join product " +
                    "on discountfreeproduct.productid = product.id " +
                "where discount.shopid = @sid",
                MapRowToDiscountFreeProduct,
                new QueryParameter("@sid", shopId)
            )).GroupBy(x => x.Discount, x => new { x.ProductAmount, x.Pid }).Select(
                x => new Discount(
                    x.Key.Id, x.Key.OffType,
                    x.Key.OffValue, x.Key.Description, x.Key.Tag,
                    x.Key.MinType, x.Key.MinValue,
                    x.Key.Is4AllProducts,
                    x.Key.ValidFrom,
                    x.Key.ValidTo,
                    x?.Where(x => x is not null && x.ProductAmount is not null).Select(x => x.ProductAmount!).Distinct()
                )
            ).OrderByDescending(x => x.ValidTo);
        }

        public async Task<IEnumerable<Discount>> FindAllActiveAsync(Guid shopId)
        {
            var dt = DateTime.Now;
            return await SearchAsync(shopId, dt, dt);
        }

        public async Task<Discount?> FindByIdAsync(Guid shopId, Guid id)
        {
            return (await template.QueryAsync(
                "select discount.id as id, offtype, offvalue, validfrom, validto, " +
                    "discount.description as description, tag, mintype, minvalue, is4allproducts, " +
                    "product.id as productid, product.name as name, product.price as price, " +
                    "product.description as productdescription, product.downloadlink as downloadlink, product.imagenr as imagenr, " +
                    "count, p.id as pid " +
                "from discount " +
                "left outer join discountfreeproduct " +
                    "on discount.id = discountfreeproduct.discountid " +
                "left outer join product " +
                    "on discountfreeproduct.productid = product.id " +
                "left outer join productdiscountproduct pdp on pdp.discountid = discount.id " +
                "left outer join product p on p.id = pdp.productid " +
                "where discount.id = @id and discount.shopid = @sid",
                MapRowToDiscountFreeProduct,
                new QueryParameter("@id", id),
                new QueryParameter("@sid", shopId)
            )).GroupBy(x => x.Discount, x => new { x.ProductAmount, x.Pid }).Select(
                x => new Discount(
                    x.Key.Id, x.Key.OffType,
                    x.Key.OffValue, x.Key.Description, x.Key.Tag,
                    x.Key.MinType, x.Key.MinValue,
                    x.Key.Is4AllProducts,
                    x.Key.ValidFrom,
                    x.Key.ValidTo,
                    x.Where(x => x is not null && x.ProductAmount is not null)!.Select(x => x.ProductAmount!),
                    x.Where(x => x is not null && x.Pid != Guid.Empty)!.Select(x => x.Pid).Distinct()
                )
            ).SingleOrDefault();
        }


        public async Task DeleteAsync(Guid id)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                //connections
                await template.ExecuteAsync(
                    "delete from discountfreeproduct " +
                    "where discountid = @did; " +
                    "delete from productdiscountproduct " +
                    "where discountid = @did1",
                    new QueryParameter("@did", id),
                    new QueryParameter("@did1", id)
                );

                //discount it self
                await template.ExecuteAsync(
                    "delete from discount where id = @id",
                    new QueryParameter("@id", id)
                );
                transactionScope.Complete();
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateAsync(Discount discount)
        {
            int i = 1;
            var sb = new StringBuilder(
                "declare @delid as uniqueidentifier = @delete; " +
                "delete from productdiscountproduct where discountid = @delid; " +
                "delete from discountfreeproduct where discountid = @delid; " +
                "update discount set " +
                    "offtype = @ot, offvalue = @ov, description = @des, tag = @tag, " +
                    "mintype = @mt, minvalue = @mv, is4allproducts = @i4p, " +
                    "validfrom = @vf, validto = @vt " +
                "where id = @id"
            );
            List<QueryParameter> queryParams = new() {
                    new QueryParameter("@delete", discount.Id),
                    new QueryParameter("@ot", (int)discount.OffType),
                    new QueryParameter("@ov", discount.OffValue),
                    new QueryParameter("@des", discount.Description),
                    new QueryParameter("@tag", discount.Tag),
                    new QueryParameter("@mt", (int)discount.MinType),
                    new QueryParameter("@mv", discount.MinValue),
                    new QueryParameter("@i4p", discount.Is4AllProducts),
                    new QueryParameter("@id", discount.Id),
                    new QueryParameter("@vf", discount.ValidFrom),
                    new QueryParameter("@vt", discount.ValidTo)
                };
            if (discount.FreeProducts != null && discount.FreeProducts.Any())
            {
                sb.Append(
                    "; insert into discountfreeproduct (discountid, productid, count) values "
                );
                foreach (var prodAmount in discount.FreeProducts)
                {
                    sb.Append($"(@did{i}, @pid{i}, @cnt{i}),");

                    queryParams.Add(new QueryParameter($"@did{i}", discount.Id));
                    queryParams.Add(new QueryParameter($"@pid{i}", prodAmount.Product.Id));
                    queryParams.Add(new QueryParameter($"@cnt{i++}", prodAmount.Count));
                }
                sb.Remove(sb.Length - 1, 1);
            }
            if (!discount.Is4AllProducts && discount.Products is not null && discount.Products.Any())
            {
                sb.Append(
                    "; insert into productdiscountproduct (discountid, productid) values "
                );
                foreach (var id in discount.Products)
                {
                    sb.Append($"(@did{i}, @pid{i}),");

                    queryParams.Add(new QueryParameter($"@did{i}", discount.Id));
                    queryParams.Add(new QueryParameter($"@pid{i++}", id));
                }
                sb.Remove(sb.Length - 1, 1);
            }

            // execute
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                await template.ExecuteAsync(sb.ToString(), queryParams.ToArray());
                transactionScope.Complete();
            }
            catch { throw; }
        }


        /// <summary>
        /// Returns all active value based discounts for the cart
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Discount>> FindValueDiscountsForCartAsync(Guid shopId, Guid cartId)
        {
            return (await template.QueryAsync(
                // select all information from discount and free products
                "select discount.id as id, offtype, offvalue, validfrom, validto, " +
                    "discount.description as description, tag, mintype, minvalue, is4allproducts, " +
                    "product.id as productid, price, name, product.description as productdescription, " +
                    "downloadlink, imagenr, count " +
                "from discount " +
                // join free products
                "left outer join discountfreeproduct on discountfreeproduct.discountid = discount.id " +
                "left outer join product on discountfreeproduct.productid = product.id " +
                // sel are the discounts which can be used for customer
                "where discount.shopid = @sid and validfrom <= @dt1 and validto >= @dt2 and mintype = 2 " + // MinType.CartValue
                    // and calculated cart sum
                    "and minvalue <= (" +
                        "select coalesce(sum(price * count), 0) " +
                        "from cartentry " +
                        "inner join product p on p.id = cartentry.productid " +
                        "where cartid = @cid" +
                    ") ",
                (IDataRecord row) => new
                {
                    Discount = MapRowToDiscount(row),
                    FreeProd = DBNull.Value.Equals(row["productid"]) ? null : new ProductAmount(
                        new Product(
                            (Guid)row["productid"],
                            (string)row["name"],
                            (decimal)row["price"],
                            (string)row["productdescription"],
                            (int)row["imagenr"],
                            (string)row["downloadlink"]
                        ), (int)row["count"]
                    )
                },
                new QueryParameter("@sid", shopId),
                new QueryParameter("@dt1", DateTime.Now),
                new QueryParameter("@dt2", DateTime.Now),
                new QueryParameter("@cid", cartId)
            )).GroupBy(x => x.Discount, x => x.FreeProd).Select(
                x => new Discount(
                    x.Key.Id, x.Key.OffType,
                    x.Key.OffValue, x.Key.Description, x.Key.Tag,
                    x.Key.MinType, x.Key.MinValue,
                    x.Key.Is4AllProducts,
                    x.Key.ValidFrom,
                    x.Key.ValidTo,
                    x.Where(x => x is not null)!
                )
            );
        }


        /// <summary>
        /// finds all discounts wich are applied to products.
        /// usefull for list views when you also wan't to show which discount is available for this product
        /// </summary>
        /// <param name="customerId">the customer id</param>
        /// <param name="productIds">all product ids</param>
        /// <returns></returns>
        public async Task<IEnumerable<DiscountLookup>> FindForProductsAsync(Guid shopId, IEnumerable<Guid> productIds)
        {
            if (!productIds.Any())
                return Enumerable.Empty<DiscountLookup>();


            //against injection, however int ranges could be modified but safe is safe

            var dt = DateTime.Now;
            var rangeParams = new List<QueryParameter>() {
                new QueryParameter("@sid", shopId),
                new QueryParameter("@dt1", dt),
                new QueryParameter("@dt2", dt)
            };
            var sbRange = new StringBuilder();

            int cnt = 0;
            foreach (var id in productIds.Distinct())
            {
                sbRange.Append($"@pid1x{cnt}, ");
                rangeParams.Add(new QueryParameter($"@pid1x{cnt++}", id));
            }

            var queryParams = rangeParams.ToArray();
            sbRange.Remove(sbRange.Length - 2, 1);

            

            // query time
            return (await template.QueryAsync(

                "select discount.id as id, offtype, offvalue, validfrom, validto, " +
                    "discount.description as description, tag, mintype, minvalue, is4allproducts, " +
                    "p2.id as productid, count, p1.id as pkey " +
                "from discount " +
                "inner join product p1 on mintype = 1 " + // MinType.ProductCount
                    "and (is4allproducts = 'true' or exists( " +
                        "select * from productdiscountproduct " +
                            "where discountid = discount.id " +
                                "and productid = p1.id " +
                    ")) " +
                "left outer join discountfreeproduct on discountfreeproduct.productid = discount.id " +
                "left outer join Product p2 on discountfreeproduct.productid = p2.id " +
                $"where discount.shopid = @sid and validfrom <= @dt1 and validto >= @dt2 and p1.id in ({sbRange})",
                (IDataRecord row) => new
                {
                    Discount = MapRowToDiscount(row),
                    FreeProd = DBNull.Value.Equals(row["productid"]) ? null : new ProductAmount(
                        new Product(
                            (Guid)row["productid"],
                            (string)row["name"],
                            (decimal)row["price"],
                            (string)row["productdescription"],
                            (int)row["imagenr"],
                            (string)row["downloadlink"]
                        ), (int)row["count"]
                    ), PKey = (Guid)row["pkey"]
                },
                queryParams
            )).GroupBy(x => x.Discount, x => new { x.FreeProd, x.PKey })
            .Select(x => new DiscountLookup(new Discount(
                    x.Key.Id, x.Key.OffType, x.Key.OffValue, x.Key.Description, x.Key.Tag,
                    x.Key.MinType, x.Key.MinValue, x.Key.Is4AllProducts,
                    x.Key.ValidFrom, x.Key.ValidTo,
                    x.Where(x => x.FreeProd is not null).Select(x => x.FreeProd!).Distinct()
                ), x.Select(x => x.PKey).Distinct())
            );
        }

        public async Task<IEnumerable<DiscountLookup>> FindProductDiscountsForCart(Guid shopId, Guid cartId)
        {
            var dt = DateTime.Now;
            return (await template.QueryAsync(

                "with entries as(" +
                    "select productid, count " +
                    "from cart inner join cartentry on cart.id = cartentry.cartid " +
                    "where cart.shopid = @sid and cart.id = @cid" +
                ") select discount.id as id, offtype, offvalue, validfrom, validto, " +
                    "discount.description as description, tag, mintype, minvalue, is4allproducts, " +
                    "p2.id as productid, discountfreeproduct.count as count, p1.id as pkey " +
                "from entries " +
                "inner join product p1 on entries.productid = p1.id " +
                "inner join discount on mintype = 1 " +
                    "and minvalue <= entries.count " +
                    "and (is4allproducts = 'true' or exists(" +
                        "select * from productdiscountproduct " +
                        "where discountid = discount.id " +
                            "and productid = p1.id" +
                    ")) " +
                "left outer join discountfreeproduct on discountfreeproduct.productid = discount.id " +
                "left outer join Product p2 on discountfreeproduct.productid = p2.id " +
                "where validfrom <= @dt1 and validto >= @dt2 and p1.id in (select productid from entries)",
                (IDataRecord row) => new
                {
                    Discount = MapRowToDiscount(row),
                    FreeProd = DBNull.Value.Equals(row["productid"]) ? null : new ProductAmount(
                        new Product(
                            (Guid)row["productid"],
                            (string)row["name"],
                            (decimal)row["price"],
                            (string)row["productdescription"],
                            (int)row["imagenr"],
                            (string)row["downloadlink"]
                        ), (int)row["count"]
                    ), PKey = (Guid)row["pkey"]
                },
                new QueryParameter("@sid", shopId),
                new QueryParameter("@cid", cartId),
                new QueryParameter("@dt1", dt),
                new QueryParameter("@dt2", dt)
            )).GroupBy(x => x.Discount, x => new { x.FreeProd, x.PKey })
            .Select(x => new DiscountLookup(new Discount(
                    x.Key.Id, x.Key.OffType, x.Key.OffValue, x.Key.Description, x.Key.Tag,
                    x.Key.MinType, x.Key.MinValue, x.Key.Is4AllProducts,
                    x.Key.ValidFrom, x.Key.ValidTo,
                    x.Where(x => x.FreeProd is not null).Select(x => x.FreeProd!).Distinct()
                ), x.Select(x => x.PKey).Distinct())
            );
        }

        public async Task<IEnumerable<Discount>> FindForProductAsync(Guid shopId, Guid productId)
        {
            var dict = await FindForProductsAsync(shopId, new Guid[] { productId });
            if(!dict.Any())
                return Enumerable.Empty<Discount>();
            return dict.Select(x => x.Discount);
        }

        public async Task<Shop?> FindShopFromDiscountAsync(Guid discountId)
        {
            return (await template.QueryAsync(
                "select shop.id as id, shop.name as name, appkey " +
                "from shop inner join discount on shop.id = discount.shopid " +
                "where discount.id = @did",
                MapRowToShop,
                new QueryParameter("@did", discountId)
            )).SingleOrDefault();
        }

        public async Task<IEnumerable<Discount>> SearchAsync(Guid shopId, DateTime from, DateTime to)
        {
            return (await template.QueryAsync(
                "select discount.id as id, offtype, offvalue, validfrom, validto, " +
                    "discount.description as description, tag, mintype, minvalue, is4allproducts, " +
                    "product.id as productid, product.name as name, product.price as price, " +
                    "product.description as productdescription, product.downloadlink as downloadlink, product.imagenr as imagenr, " +
                    "count, null as pid " +
                "from discount " +
                "left outer join discountfreeproduct " +
                    "on discount.id = discountfreeproduct.discountid " +
                "left outer join product " +
                    "on discountfreeproduct.productid = product.id " +
                "where discount.shopid = @sid and (" +
                    "discount.validfrom >= @dtfrom1 and discount.validto <= @dtto1 " +
                    "or discount.validto >= @dtfrom2 and discount.validfrom <= @dtfrom3 " +
                    "or discount.validfrom <= @dtto2 and discount.validto >= @dtto3" +
                ")",
                MapRowToDiscountFreeProduct,
                new QueryParameter("@sid", shopId),
                new QueryParameter("@dtfrom1", from),
                new QueryParameter("@dtto1", to),
                new QueryParameter("@dtfrom2", from),
                new QueryParameter("@dtfrom3", from),
                new QueryParameter("@dtto2", to),
                new QueryParameter("@dtto3", to)
            )).GroupBy(x => x.Discount, x => new { x.ProductAmount, x.Pid }).Select(
                x => new Discount(
                    x.Key.Id, x.Key.OffType,
                    x.Key.OffValue, x.Key.Description, x.Key.Tag,
                    x.Key.MinType, x.Key.MinValue,
                    x.Key.Is4AllProducts,
                    x.Key.ValidFrom,
                    x.Key.ValidTo,
                    x?.Where(x => x is not null && x.ProductAmount is not null).Select(x => x.ProductAmount!).Distinct()
                )
            ).OrderByDescending(x => x.ValidTo);
        }

        public async Task<IEnumerable<Discount>> SearchAsync(Guid shopId, string pattern, DateTime from, DateTime to)
        {
            return (await template.QueryAsync(
                "select * from( " +
                    "select discount.id as id, offtype, offvalue, validfrom, validto, " +
                        "discount.description as description, tag, mintype, minvalue, is4allproducts, " +
                        "product.id as productid, product.name as name, product.price as price, " +
                        "product.description as productdescription, product.downloadlink as downloadlink product.imagenr as imagenr, " +
                        "count, null as pid, difference(discount.description, @pattern) as value " +
                    "from discount " +
                    "left outer join discountfreeproduct " +
                        "on discount.id = discountfreeproduct.discountid " +
                    "left outer join product " +
                        "on discountfreeproduct.productid = product.id " +
                    "where discount.shopid = @sid and (" +
                        "discount.validfrom >= @dtfrom1 and discount.validto <= @dtto1 " +
                        "or discount.validto >= @dtfrom2 and discount.validfrom <= @dtfrom3 " +
                        "or discount.validfrom <= @dtto2 and discount.validto >= @dtto3 " +
                    ")" +
                ") sel where value > 2",
                row => new
                {
                    Dfp = MapRowToDiscountFreeProduct(row),
                    Value = (int)row["value"]
                },
                new QueryParameter("@sid", shopId),
                new QueryParameter("@pattern", pattern),
                new QueryParameter("@dtfrom1", from),
                new QueryParameter("@dtto1", to),
                new QueryParameter("@dtfrom2", from),
                new QueryParameter("@dtfrom3", from),
                new QueryParameter("@dtto2", to),
                new QueryParameter("@dtto3", to)
            )).GroupBy(x => new { x.Dfp.Discount, x.Value }, x => new { x.Dfp.ProductAmount, x.Dfp.Pid }).OrderByDescending(x => x.Key.Value).Select(
                x => new Discount(
                    x.Key.Discount.Id, x.Key.Discount.OffType,
                    x.Key.Discount.OffValue, x.Key.Discount.Description, x.Key.Discount.Tag,
                    x.Key.Discount.MinType, x.Key.Discount.MinValue,
                    x.Key.Discount.Is4AllProducts,
                    x.Key.Discount.ValidFrom,
                    x.Key.Discount.ValidTo,
                    x?.Where(x => x is not null && x.ProductAmount is not null).Select(x => x.ProductAmount!).OrderBy(x => x.Product.Name).Distinct()
                )
            );
        }
    }
}
