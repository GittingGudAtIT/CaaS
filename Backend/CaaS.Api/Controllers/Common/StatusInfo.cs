using CaaS.Core.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace CaaS.Api.Controllers.Common
{
    public static class StatusInfo
    {
        public static ProblemDetails InvalidShopId(Guid shopId)
        {
            return new()
            {
                Title = "Invalid shop ID",
                Detail = $"Shop with ID '{shopId}' does not exist"
            };
        }

        public static ProblemDetails InvalidOrderId(Guid orderId)
        {
            return new()
            {
                Title = "Invalid order ID",
                Detail = $"Order with ID '{orderId}' does not exist"
            };
        }

        public static ProblemDetails InvalidProductIdRange()
        {
            return new()
            {
                Title = "Invalid product ID range",
                Detail = $"Sent id range was 'null' or empty"
            };
        }

        public static ProblemDetails InvalidCartId(Guid routeId, Guid? dtoId)
        {
            return new()
            {
                Title = "Cart Id conflict",
                Detail = $"route id {routeId} does not match with body id {dtoId}"
            };
        }

        public static ProblemDetails InvalidDiscountConfiguration()
        {
            return new()
            {
                Title = "Invalid discount configuration",
                Detail = "Invalid discount configuration"
            };
        }

        public static ProblemDetails AmountMustBeGreaterThanZero()
        {
            return new()
            {
                Title = "Invalid amount",
                Detail = "Amounts can't be smaller than '1'"
            };
        }

        public static ProblemDetails StringEmpty(string paramName)
        {
            return new()
            {
                Title = $"{paramName} must not be empty",
                Detail = $"{paramName} was empty"
            };
        }

        public static ProblemDetails PriceCantBeSmallerThanZero()
        {
            return new()
            {
                Title = "Product Price can't be less than '0'",
                Detail = "Product Price can't be less than '0'"
            };
        }
    }
}
