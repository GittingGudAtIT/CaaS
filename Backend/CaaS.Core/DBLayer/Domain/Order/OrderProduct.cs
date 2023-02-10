using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Domain
{
    public class OrderProduct
    {
        public OrderProduct(Guid originalId, string name, decimal price)
        {
            OriginalId = originalId;
            Name = name;
            Price = price;
        }

        public Guid OriginalId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public override int GetHashCode()
        {
            return OriginalId.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj is OrderProduct op
                && op.OriginalId == OriginalId
                && op.Name == Name
                && op.Price >= Price - 0.001M && op.Price <= Price + 0.001M;
        }
    }
}
