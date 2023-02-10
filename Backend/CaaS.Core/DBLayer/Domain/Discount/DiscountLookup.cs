using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Domain
{
    public class DiscountLookup
    {
        public DiscountLookup(Discount discount, IEnumerable<Guid> productIds)
        {
            Discount = discount;
            ProductIds = productIds;
        }

        public Discount Discount { get; set; }
        public IEnumerable<Guid> ProductIds { get; set; }
    }
}
