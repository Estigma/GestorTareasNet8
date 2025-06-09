using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using TaskService.Data;
using TaskService.Interfaces;
using TaskService.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Exceptions;
using Polly.Retry;
using TaskService.Policies;


var builder = WebApplication.CreateBuilder(args);

var seqUrl = builder.Configuration["SEQ_URL"] ?? "http://localhost:5341";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .Enrich.WithMachineName()
    .WriteTo.Console()
    .WriteTo.Seq(seqUrl)
    .CreateLogger();

Log.Information("Serilog configurado para enviar logs a: {SeqUrl}", seqUrl);


builder.Host.UseSerilog();

if (builder.Configuration["ASPNETCORE_ENVIRONMENT"] != "Test")
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

builder.Services.AddScoped<ITareaRepository, TareaRepository>();
builder.Services.AddSingleton<IMessageProducer, RabbitMQProducer>();
builder.Services.AddSingleton<AsyncRetryPolicy<string>>(sp =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("PollyPolicies");
    return PollyPolicies.GetFtpRetryPolicy(logger);
});
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

var jaegerEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317";
var serviceName = builder.Environment.ApplicationName;

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: builder.Environment.ApplicationName,
                    serviceVersion: "1.0.0"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("TaskService.RabbitMQProducer")
                .AddSource("TaskService.UsuarioValidator")
                .AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddProcessInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317");
                });
    });

var app = builder.Build();

app.UseSerilogRequestLogging();

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