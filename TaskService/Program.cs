using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using TaskService.Data;
using TaskService.Interfaces;
using TaskService.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration["ASPNETCORE_ENVIRONMENT"] != "Test")
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITareaRepository, TareaRepository>();
builder.Services.AddSingleton<IMessageProducer, RabbitMQProducer>();
builder.Services.AddScoped<IFtpService, FtpService>();
builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    return new ConnectionFactory()
    {
        HostName = builder.Configuration["RabbitMQ:HostName"],
        Port = AmqpTcpEndpoint.UseDefaultPort,
        UserName = builder.Configuration["RabbitMQ:Username"],
        Password = builder.Configuration["RabbitMQ:Password"],
        DispatchConsumersAsync = true,
        AutomaticRecoveryEnabled = true,
        NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
    };
});

builder.Services.AddScoped<IUsuarioValidator, UsuarioValidatorRabbitMQ>();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment() && !app.Environment.ApplicationName.Contains("Test"))
{    
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }