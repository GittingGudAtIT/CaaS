using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CaaS.Core.Domain;

namespace CaaS.Core.DBLayer.Domain.TestExtensions
{
    public static class TestExtensions
    {
        public static bool FullyEquals(this Discount d, object? obj)
        {
            return obj is Discount discount
                && discount.Id == d.Id
                && discount.OffType == d.OffType
                && discount.OffValue >= d.OffValue - 0.00001M && discount.OffValue <= d.OffValue + 0.00001M
                && discount.Description == d.Description
                && discount.Tag == d.Tag
                && discount.MinType == d.MinType
                && discount.MinValue >= d.MinValue - 0.001M && discount.MinValue <= d.MinValue + 0.001M
                && discount.Is4AllProducts == d.Is4AllProducts
                && discount.ValidFrom >= d.ValidFrom.AddMilliseconds(-2) && discount.ValidFrom <= d.ValidFrom.AddMilliseconds(2)
                        && discount.ValidTo >= d.ValidTo.AddMilliseconds(-2) && discount.ValidTo <= d.ValidTo.AddMilliseconds(2)
                && (discount.FreeProducts is null && d.FreeProducts is null
                    || discount.FreeProducts is not null && d.FreeProducts is not null
                    && discount.FreeProducts.All(x => d.FreeProducts.Contains(x))
                );
        }

        public static bool FullyEquals(this Product p, object? obj)
        {
            return obj is Product product
                && product.Id == p.Id
                && product.Name == p.Name
                && product.Price >= p.Price - 0.001M && product.Price <= p.Price + 0.001M
                && product.Description == p.Description
                && product.DownloadLink == p.DownloadLink
                && product.ImageNr == p.ImageNr;
        }

        public static bool FullyEquals(this Order o, object? obj)
        {
            return obj is Order order
                && order.Id == o.Id
                && order.DateTime >= o.DateTime.AddMilliseconds(-2) && order.DateTime <= o.DateTime.AddMilliseconds(2)
                && order.OffSum <= o.OffSum + 0.001M && order.OffSum >= o.OffSum - 0.001M
                && order.Customer.Equals(o.Customer)
                && order.Entries.All(x => o.Entries.Contains(x))
                && order.Total == o.Total
                && order.DownloadLink == o.DownloadLink;
        }

        public static bool FullyEquals(this Shop s, object? obj)
        {
            return obj is Shop shop
                && shop.Id == s.Id
                && shop.Name == s.Name;
        }

    }
}
