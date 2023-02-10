using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Dal.Common
{
    public interface IConnectionFactory
    {
        Task<DbConnection> CreateConnectionAsync();
    }
}
