using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Domain
{
    public class Customer
    {
        public Customer(string firstname, string lastname, string email)
        {
            Firstname = firstname;
            Lastname = lastname;
            Email = email;
        }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public override string ToString() => $"{Firstname} {Lastname}";
        public override bool Equals(object? obj)
        {
            return obj is Customer customer
                && customer.Firstname == Firstname
                && customer.Lastname == Lastname
                && customer.Email == Email;
        }
        public override int GetHashCode()
        {
            return Email.GetHashCode();
        }
    }
}
