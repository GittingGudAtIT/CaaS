using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CaaS.Core.Dal.Common
{
    public delegate T RowMapper<T>(IDataRecord row);

    public class AdoTemplate
    {
        private readonly IConnectionFactory connectionFactory;

        public AdoTemplate(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, RowMapper<T> rowMapper, params QueryParameter[] parameters)
        {
            await using DbConnection connection = await connectionFactory.CreateConnectionAsync();

            await using DbCommand command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameters(command, parameters);

            await using DbDataReader reader = await command.ExecuteReaderAsync();

            var items = new List<T>();
            while (await reader.ReadAsync())
                items.Add(rowMapper(reader));

            return items;
        }

        public async Task<int> ExecuteAsync(string sql, params QueryParameter[] parameters)
        {
            await using DbConnection connection = await connectionFactory.CreateConnectionAsync();

            await using DbCommand command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameters(command, parameters);

            return await command.ExecuteNonQueryAsync();
        }

        public async Task<T?> ExecuteScalarAsync<T>(string sql, params QueryParameter[] parameters)
        {
            await using DbConnection connection = await connectionFactory.CreateConnectionAsync();

            await using DbCommand command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameters(command, parameters);

            return (T?)await command.ExecuteScalarAsync();
        }

        private static void AddParameters(DbCommand command, IEnumerable<QueryParameter> parameters)
        {
            foreach (var p in parameters)
            {
                DbParameter dbParameter = command.CreateParameter();
                dbParameter.ParameterName = p.Name;
                dbParameter.Value = p.Value;
                command.Parameters.Add(dbParameter);
            }
        }
    }
}
