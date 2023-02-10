using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Domain
{
    public class Product
    {
        public Product(Guid id, string name, decimal price, string description, int imageNr = 0, string? downloadLink = null)
        {
            Name = name;
            Id = id;
            Description = description;
            DownloadLink = downloadLink?? string.Empty;
            Price = price;
            ImageNr = imageNr;
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string DownloadLink { get; set; }
        public int ImageNr { get; set; }
        public override string ToString()
        {
            return Name;
        }
        public override bool Equals(object? obj)
        {
            return obj is Product product && product.Id == Id;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
