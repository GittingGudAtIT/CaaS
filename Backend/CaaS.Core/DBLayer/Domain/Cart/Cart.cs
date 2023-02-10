using CaaS.Core.DBLayer.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Domain
{
    public class Cart : List<ProductAmount>
    {
        public Cart(Guid id) : base() { Id = id; }
        public Cart(Guid id, IEnumerable<ProductAmount> entries) : base(entries) { Id = id; }
        public Guid Id { get; set; }
        public CartDiscounts? CartDiscounts { get; set; }
        public decimal EvaluateAmount()
        {
            decimal sum = 0;

            foreach (var entry in this)
            {
                var entryValue = entry.Product.Price * entry.Count;
                sum += entryValue;

                if(CartDiscounts?.ProductDiscounts is not null)
                {
                    var discounts = CartDiscounts.ProductDiscounts
                        .Where(x => x.ProductIds.Contains(entry.Product.Id))
                        .Select(x => x.Discount);

                    foreach (var d in discounts)
                    {
                        if (d.OffType == OffType.Percentual)
                            sum -= entryValue * d.OffValue;
                        else if (d.OffType == OffType.Fixed)
                            sum -= d.OffValue * (int)(entry.Count / d.MinValue);
                    }
                }
            }

            var sumSave = sum;
            if(CartDiscounts?.ValueDiscounts is not null)
            {
                foreach(var d in CartDiscounts.ValueDiscounts)
                {
                    if (d.OffType == OffType.Fixed)
                        sum -= sumSave * (int)(sumSave / d.OffValue);
                    if(d.OffType == OffType.Percentual)
                        sum -= sumSave * d.OffValue;
                }
            }
            return sum;
        }
    }
}
