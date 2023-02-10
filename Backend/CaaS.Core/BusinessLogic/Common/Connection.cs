using CaaS.Core.Dal.Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.BusinessLogic.Common
{
    internal static class Connection
    {
        public static IConnectionFactory GetFactory()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            return DefaultConnectionFactory.FromConfiguration(configuration, "CaaSDbConnection");
        }
    }
}
