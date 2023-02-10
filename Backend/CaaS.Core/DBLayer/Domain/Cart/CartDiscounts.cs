using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Domain
{
    public class CartDiscounts
    {
        public CartDiscounts(
            IEnumerable<DiscountLookup>? productDiscounts = null, 
            IEnumerable<Discount>? valueDiscounts = null)
        {
            ProductDiscounts = productDiscounts;
            ValueDiscounts = valueDiscounts;
        }

        public IEnumerable<DiscountLookup>? ProductDiscounts { get; set; }
        public IEnumerable<Discount>? ValueDiscounts { get; set; }
    }
}
