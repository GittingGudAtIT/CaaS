using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Domain
{
    public class ProductAmount
    {
        public ProductAmount(Product product, int count)
        {
            Product = product;
            Count = count;
        }

        public Product Product { get; set; }
        public int Count { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ProductAmount pa
                && pa.Count == Count
                && pa.Product.Equals(Product);
        }
        public override int GetHashCode()
        {
            return Product.GetHashCode()
                + Count * Count;
        }
    }
}
