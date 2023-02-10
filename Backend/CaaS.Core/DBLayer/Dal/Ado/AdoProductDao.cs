using CaaS.Core.Dal.Common;
using CaaS.Core.Dal.Interface;
using CaaS.Core.Domain;
using Org.BouncyCastle.Asn1.Pkcs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Dal.Ado
{
    public class AdoProductDao : IProductDao
    {
        private readonly AdoTemplate template;

        public AdoProductDao(IConnectionFactory connectionFactory)
        {
            template = new AdoTemplate(connectionFactory);
        }

        private Shop MapRowToShop(IDataRecord row)
        {
            return new(
                (Guid)row[nameof(Shop.Id)],
                (string)row[nameof(Shop.Name)],
                (string)row[nameof(Shop.AppKey)]
            );
        }

        private Product MapRowToProduct(IDataRecord row)
        {
            return new(
                (Guid)row["id"],
                (string)row["name"],
                (decimal)row["price"],
                (string)row["description"],
                (int)row["imagenr"],
                (string)row["downloadlink"]
            );
        }

        public async Task DeleteAsync(Guid productId)
        {
            await template.ExecuteAsync(
                "delete from product where id = @pid",
                new QueryParameter("@pid", productId)
            );
        }

        public async Task<IEnumerable<Product>> FindAllAsync(Guid shopId)
        {
            return await template.QueryAsync(
                "select * from product " +
                "where shopid = @sid",
                MapRowToProduct,
                new QueryParameter("@sid", shopId)
            );
        }

        public async Task<IEnumerable<Product>> SearchAsync(Guid shopId, string pattern)
        {
            return (await template.QueryAsync(
                "declare @p as varchar(255) = @pattern; " +
                "select * from (" +
                    "select *, (" +
                        "case when convert(nvarchar(36), id) = @p then 100 when charindex(@p, id) > 0 then 8 else 0 end + " +
                        "case when @p = name then 12 else difference(name, @p) end + " +
                        "case when @p = description then 12 else difference(name, @p) end " +
                    ") as value " +
                    "from product " +
                    "where shopid = @sid " +
                ") q " +
                "where value > 4",
                (row) => new
                {
                    Product = MapRowToProduct(row),
                    Value = (int)row["value"]
                },
                new QueryParameter("@pattern", pattern),
                new QueryParameter("@sid", shopId)
            )).OrderByDescending(x => x.Value).Select(x => x.Product); 
        }

        public async Task<Product?> FindByIdAsync(Guid shopId, Guid id)
        {
            return (await template.QueryAsync(
                "select * from product " +
                "where id = @id and shopid = @sid",
                MapRowToProduct,
                new QueryParameter("@id", id),
                new QueryParameter("@sid", shopId)
            )).FirstOrDefault();
        }

        public async Task InsertAsync(Guid shopId, Product product)
        {
            product.Id = await template.ExecuteScalarAsync<Guid>(
                "declare @guid uniqueidentifier = newid();" +
                "insert into product (id, shopid, name, price, description, downloadlink, imagenr) " +
                "values (@guid, @sid, @nm, @pr, @des, @dl, @in); " +
                "select @guid",
                new QueryParameter("@sid", shopId),
                new QueryParameter("@nm", product.Name),
                new QueryParameter("@pr", product.Price),
                new QueryParameter("@des", product.Description),
                new QueryParameter("@dl", product.DownloadLink),
                new QueryParameter("@in", product.ImageNr)
            );
        }

        public async Task UpdateAsync(Product product)
        {
            await template.ExecuteAsync(
                "update product " +
                "set price = @pr " +
                "where id = @pid",
                new QueryParameter("@pr", product.Price),
                new QueryParameter("@pid", product.Id)
            );
        }

        public async Task<Shop?> FindShopFromProductAsync(Guid productId)
        {
            return (await template.QueryAsync(
                "select shop.id as id, shop.name as name, appkey " +
                "from shop inner join product on shop.id = product.shopid " +
                "where product.id = @pid",
                MapRowToShop,
                new QueryParameter("@pid", productId)
            )).SingleOrDefault();
        }

        public async Task<IEnumerable<Product>> FindByIdAsync(Guid shopId, IEnumerable<Guid> productIds)
        {
            if (!productIds.Any())
                return Enumerable.Empty<Product>();

            var sb = new StringBuilder(
                "select * from product " +
                "where shopid = @sid and id in (");

            var queryParams = new List<QueryParameter>()
            {
                new QueryParameter("@sid", shopId)
            };

            int i = 0;
            foreach (var productId in productIds.Distinct())
            {
                sb.Append($"@id{i}, ");
                queryParams.Add(new QueryParameter($"@id{i++}", productId));
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append(')');

            return await template.QueryAsync(sb.ToString(),
                MapRowToProduct,
                queryParams.ToArray()
            );
        }
    }
}
