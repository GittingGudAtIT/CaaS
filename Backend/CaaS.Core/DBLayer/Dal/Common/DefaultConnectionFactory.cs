using System.Configuration;
using System.Data.Common;
using Microsoft.Extensions.Configuration;

namespace CaaS.Core.Dal.Common;

public class DefaultConnectionFactory : IConnectionFactory
{
    private readonly DbProviderFactory dbProviderFactory;

    public static IConnectionFactory FromConfiguration(IConfiguration config, string connectionStringConfigName)
    {
        var connectionConfig = config.GetSection("ConnectionStrings").GetSection(connectionStringConfigName);
        string? connectionString = connectionConfig["ConnectionString"];
        string? providerName = connectionConfig["ProviderName"];

        return new DefaultConnectionFactory(connectionString, providerName);
    }

    public DefaultConnectionFactory(string? connectionString, string? providerName)
    {
        ConnectionString = connectionString ?? "";

        ProviderName = providerName ?? "";

        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", Microsoft.Data.SqlClient.SqlClientFactory.Instance);
        DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySql.Data.MySqlClient.MySqlClientFactory.Instance);

        dbProviderFactory = DbProviderFactories.GetFactory(ProviderName);
    }

    public string ConnectionString { get; }

    public string ProviderName { get; }

    public async Task<DbConnection> CreateConnectionAsync()
    {
        var connection = dbProviderFactory.CreateConnection();
        if (connection is null)
            throw new InvalidOperationException("DbProviderFactory.CreateConnection() returned null");

        connection.ConnectionString = ConnectionString;
        await connection.OpenAsync();
        return connection;
    }
}