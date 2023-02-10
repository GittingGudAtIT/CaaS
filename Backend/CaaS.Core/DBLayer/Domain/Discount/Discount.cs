using CaaS.Core.Domain;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Domain
{
    public enum OffType { None = 0, Fixed = 1, Percentual = 2, FreeProduct = 3 };
    public enum MinType { ProductCount = 1, CartSum = 2 }

    public class Discount
    {
        public Discount() { }
        public Discount(
            Guid id,
            OffType offType, decimal offValue,
            string description,
            string tag,
            MinType minType, decimal minValue,
            bool is4AllProducts,
            DateTime validFrom,
            DateTime validTo,
            IEnumerable<ProductAmount>? freeProducts = null,
            IEnumerable<Guid>? products = null
        )
        {
            Id = id;
            OffType = offType;
            OffValue = offValue;
            Tag = tag;
            Description = description;
            MinType = minType;
            MinValue = minValue;
            Is4AllProducts = is4AllProducts;
            ValidFrom = validFrom;
            ValidTo = validTo;
            if (freeProducts != null)
                FreeProducts = freeProducts;
            if (products != null)
                Products = products;
        }

        public Guid Id { get; set; }
        public OffType OffType { get; set; }
        public decimal OffValue { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public MinType MinType { get; set; }
        public decimal MinValue { get; set; }
        public bool Is4AllProducts { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public IEnumerable<ProductAmount> FreeProducts { get; set; } = Enumerable.Empty<ProductAmount>();
        public IEnumerable<Guid> Products { get; set; } = Enumerable.Empty<Guid>();

        public bool IsValid()
        {
            return ValidFrom < ValidTo
                && !(
                    OffType == OffType.FreeProduct && MinType == MinType.CartSum
                ) && !(MinType == MinType.ProductCount
                    && (Products.IsNullOrEmpty() && !Is4AllProducts
                    || Products.Any() && Is4AllProducts)
                ) && !(
                    OffType == OffType.None && FreeProducts.IsNullOrEmpty()
                ) && Products.Distinct().Count() == Products.Count()
                && !(
                   OffType != OffType.None && OffValue <= 0
                ) && !(
                    MinType == MinType.CartSum && (
                        MinValue <= 0
                        || Is4AllProducts
                        || Products.Any()
                    ) || MinType == MinType.ProductCount && MinValue < 1
                ) && !(
                    OffType == OffType.FreeProduct && OffValue < 1
                ) && FreeProducts.Select(x => x.Product.Id).Distinct().Count() == FreeProducts.Count()
                && FreeProducts.All(pa => pa.Count > 0)
                && !(OffType == OffType.Percentual && OffValue > 1);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj is Discount discount && discount.Id == Id;
        }
    }
}
