using CaaS.Core.DBLayer.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Domain
{
    public class Order
    {
        public Order(Guid id, DateTime dateTime, decimal offSum, Customer customer, IEnumerable<OrderEntry> entries, decimal total, string downloadLink)
        {
            Id = id;
            DateTime = dateTime;
            OffSum = offSum;
            Customer = customer;
            Entries = entries;
            Total = total;
            DownloadLink = downloadLink;
        }

        public Order(Guid id, DateTime dateTime, decimal offSum, Customer customer, IEnumerable<OrderEntry> entries, string downloadLink)
        {
            Id = id;
            DateTime = dateTime;
            OffSum = offSum;
            Customer = customer;
            Entries = entries;
            Total = entries.Sum(e => e.Count * e.Product.Price) - offSum;
            DownloadLink = downloadLink;
        }

        /// <summary>
        /// this can be used to create orders directly from the customer's cart
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dateTime"></param>
        /// <param name="customer"></param>
        /// <exception cref="InvalidOperationException">is thrown when customer.Cart is null or empty</exception>
        /// <exception cref="NotImplementedException"></exception>
        public Order(Guid id, DateTime dateTime, Customer customer, Cart cart)
        {
            Id = id;
            DateTime = dateTime;
            Customer = customer;
            int rowNr = 0;

            var entries = new List<OrderEntry>();
            decimal offsum = 0;

            if (!cart.Any())
                throw new InvalidOperationException("cart must not be empty");

            //product discounts
            foreach(var entry in cart)
            {
                var prod = new OrderProduct(entry.Product.Id, entry.Product.Name, entry.Product.Price);
                entries.Add(new OrderEntry(++rowNr, entry.Count, prod));

                if(cart.CartDiscounts?.ProductDiscounts != null)
                {
                    foreach(var disLU in cart.CartDiscounts.ProductDiscounts)
                    {
                        if (disLU.ProductIds.Contains(prod.OriginalId))
                        {
                            var discount = disLU.Discount;
                            int multiplicator = entry.Count / (int)discount.MinValue;

                            switch (discount.OffType)
                            {
                                case OffType.Percentual:
                                    offsum += entry.Count * prod.Price * discount.OffValue;
                                    break;
                                case OffType.FreeProduct:
                                    entries.Add(new OrderEntry(
                                        ++rowNr,
                                        multiplicator,
                                        new OrderProduct(prod.OriginalId, prod.Name, 0))
                                    );
                                    offsum += (int)discount.OffValue * prod.Price * multiplicator;
                                    break;
                                case OffType.Fixed:
                                    offsum += discount.OffValue * multiplicator;
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                            if (discount.FreeProducts != null)
                            {
                                foreach (var freeProd in discount.FreeProducts)
                                {
                                    entries.Add(new OrderEntry(++rowNr, freeProd.Count * multiplicator,
                                        new OrderProduct(freeProd.Product.Id, freeProd.Product.Name, 0))
                                    );
                                    offsum += freeProd.Count * freeProd.Product.Price * multiplicator;
                                }
                            }
                        }
                    }
                }
            }
            

            if(cart.CartDiscounts?.ValueDiscounts != null)
            {
                decimal cartRawValue = cart.Sum(x => x.Count * x.Product.Price);

                //cart / value discounts
                foreach (var discount in cart.CartDiscounts.ValueDiscounts)
                {
                    int multiplicator = (int)(cartRawValue / discount.MinValue);
                    switch (discount.OffType)
                    {
                        case OffType.Percentual:
                            offsum += cartRawValue * discount.OffValue;
                            break;
                        case OffType.Fixed:
                            offsum += multiplicator * discount.OffValue;
                            break;
                        default:
                            if (discount.OffType == OffType.None && discount.FreeProducts is not null && discount.FreeProducts.Any())
                                break;
                            throw new InvalidOperationException();
                    }
                    if (discount.FreeProducts != null)
                    {
                        foreach (var freeProd in discount.FreeProducts)
                        {
                            entries.Add(new OrderEntry(++rowNr, freeProd.Count * multiplicator,
                                new OrderProduct(freeProd.Product.Id, freeProd.Product.Name, 0))
                            );
                            offsum += freeProd.Count * freeProd.Product.Price * multiplicator;
                        }
                    }
                }
            }

            OffSum = offsum;
            Entries = entries;
            Total = entries.Sum(e => e.Count * e.Product.Price) - offsum;
            DownloadLink = $"https://caas/temporaldownload/{Guid.NewGuid()}";
        }

        public string DownloadLink { get; set; }
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }
        public decimal OffSum { get; set; }
        public Customer Customer { get; set; }
        public decimal Total { get; set; }
        public IEnumerable<OrderEntry> Entries { get; set; }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            return obj is Order order && order.Id == Id;
        }
    }
}
