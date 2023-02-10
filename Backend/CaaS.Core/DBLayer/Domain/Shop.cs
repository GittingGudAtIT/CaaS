using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Domain
{
    public class Shop
    {
        public Shop() { }
        public Shop(Guid id, string name, string appKey)
        {
            Id = id;
            AppKey = appKey;
            Name = name;
        }
        public Guid Id { get; set; }
        public string AppKey { get; set; } = null!;
        public string Name { get; set; } = null!;

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object? obj)
        {
            return obj is Shop shop && shop.Id == Id;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
