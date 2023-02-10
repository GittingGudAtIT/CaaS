using CaaS.Core.Dal.Common;
using CaaS.Core.Dal.Interface;
using CaaS.Core.Domain;
using Org.BouncyCastle.Crypto.Modes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace CaaS.Core.Dal.Ado
{
    public class AdoCartDao : ICartDao
    {
        private readonly AdoTemplate template;

        ProductAmount MapRowToProductAmount(IDataRecord row)
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

        public AdoCartDao(IConnectionFactory connectionFactory)
        {
            template = new AdoTemplate(connectionFactory);
        }

        public async Task DeleteAsync(Guid shopId, Guid cartId)
        {
            await template.ExecuteAsync(
                "delete from cart " +
                "where id = @cid and shopid = @sid",
                new QueryParameter("@cid", cartId),
                new QueryParameter("@sid", shopId)
            );
        }

        public async Task<Cart?> FindByIdAsync(Guid shopId, Guid cartId)
        {
            var entries = await template.QueryAsync(
                "select * from cartentry " +
                "inner join product on cartentry.productid = product.id " +
                "where cartid = @cid and shopid = @sid",
                MapRowToProductAmount,
                new QueryParameter("@cid", cartId),
                new QueryParameter("@sid", shopId)
            );
            if (entries.Any())
                return new Cart(cartId, entries);
            else return null;
        }

        public async Task InsertAsync(Guid shopId, Cart cart)
        {
            cart.Id = await template.ExecuteScalarAsync<Guid>(
                "declare @guid uniqueidentifier = newid();" +
                "insert into cart (id, shopid) values (@guid, @sid);" +
                "select @guid",
                new QueryParameter("@sid", shopId)
            );

            if (cart.Any()) { 
                var sb = new StringBuilder(
                    "insert into cartentry (cartid, count, productid) values "
                );
                var queryParams = new List<QueryParameter>();
                int i = 1;
                foreach (var entry in cart)
                {
                    queryParams.Add(new QueryParameter($"@cid{i}", cart.Id));
                    queryParams.Add(new QueryParameter($"@cnt{i}", entry.Count));
                    queryParams.Add(new QueryParameter($"@pid{i}", entry.Product.Id));

                    sb.Append($"(@cid{i}");
                    sb.Append($", @cnt{i}");
                    sb.Append($", @pid{i++}),");
                }
                sb.Remove(sb.Length - 1, 1);

                await template.ExecuteAsync(
                    sb.ToString(),
                    queryParams.ToArray()
                );
            }
        }

        public async Task UpdateAsync(Guid shopId, Cart cart)
        {
            var shop = await FindShopFromCartAsync(cart.Id);
            if(shop is not null && shop.Id == shopId)
            {
                var task = template.ExecuteAsync(
                    "delete from cartentry where cartid = @cid",
                    new QueryParameter("@cid", cart.Id)
                );

                var queryParams = new List<QueryParameter>(cart.Count);
                var sb = new StringBuilder(
                    "insert into cartentry (cartid, productid, count) " +
                    "values "
                );

                int i = 0;
                foreach (var entry in cart)
                {
                    sb.Append($"(@cid{i}, @pid{i}, @cnt{i}),");
                    queryParams.Add(new QueryParameter($"@cid{i}", cart.Id));
                    queryParams.Add(new QueryParameter($"@pid{i}", entry.Product.Id));
                    queryParams.Add(new QueryParameter($"@cnt{i++}", entry.Count));
                }
                sb.Remove(sb.Length - 1, 1);

                await task;
                await template.ExecuteAsync(
                    sb.ToString(),
                    queryParams.ToArray()
                );
            }
        }


        public async Task<Shop?> FindShopFromCartAsync(Guid cartId)
        {
            return (await template.QueryAsync(
                "select shop.id as id, shop.name as name, appkey " +
                "from shop inner join cart on shop.id = cart.shopid " +
                "where cart.id = @cid",
                MapRowToShop,
                new QueryParameter("@cid", cartId)
            )).SingleOrDefault();
        }
    }
}
