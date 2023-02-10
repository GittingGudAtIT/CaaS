

using CaaS.Core.BusinessLogic.Common;
using CaaS.Core.Dal.Common;
using CaaS.Core.DB;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Drawing.Imaging;
using System.IO;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionFactory = DefaultConnectionFactory.FromConfiguration(configuration, "CaaSDbConnection");

var manager = new DbManager(connectionFactory);

await manager.CreateAsync();
#if DEBUG
await manager.InsertSampleDataAsync();
await manager.InsertSampleImagesAsync();
#endif

