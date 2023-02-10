using CaaS.Core.BusinessLogic.Implementation;
using CaaS.Core.BusinessLogic.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options => options.ReturnHttpNotAcceptable = true)
    .AddNewtonsoftJson()
    .AddXmlDataContractSerializerFormatters();

// Controllers
builder.Services.AddScoped<IShopManagementLogic, ShopManagementLogic>();
builder.Services.AddScoped<IProductManagementLogic, ProductManagementLogic>();
builder.Services.AddScoped<ICartManagementLogic, CartManagementLogic>();
builder.Services.AddScoped<IDiscountManagementLogic, DiscountManagementLogic>();

// Every route is lowercase now
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddAutoMapper(typeof(Program));

// Cors n' stuff
builder.Services.AddCors(builder =>
    builder.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()));

builder.Services.AddOpenApiDocument(options =>
    options.PostProcess = document => document.Info.Title = "CaaS API"
);

var app = builder.Build();

// Configure the HTTP request pipeline.

// Interface documentation for developers
app.UseOpenApi();
app.UseSwaggerUi3(settings => settings.Path = "/swagger");
app.UseReDoc(settings => settings.Path = "/redoc");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseCors();

app.Run();