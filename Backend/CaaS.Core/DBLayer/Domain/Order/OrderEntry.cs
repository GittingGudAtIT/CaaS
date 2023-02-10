using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Domain
{
    public class OrderEntry
    {
        public OrderEntry(int rowNumber, int count, OrderProduct product)
        {
            RowNumber = rowNumber;
            Count = count;
            Product = product;
        }

        public int RowNumber { get; set; }
        public int Count { get; set; }
        public OrderProduct Product { get; set; }

        public override int GetHashCode()
        {
            return Product.GetHashCode() + RowNumber * 13;
        }

        public override bool Equals(object? obj)
        {
            return obj is OrderEntry oe
                && oe.RowNumber == RowNumber
                && oe.Count == Count
                && oe.Product.Equals(Product);
        }
    }
}
