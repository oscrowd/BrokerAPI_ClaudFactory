using BrokerAPIClaudFactory.Broker;

namespace BrokerAPIClaudFactory
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Конфигурация из appsettings.json
            var brokerConfig = builder.Configuration.GetSection("Broker");

            // Add services to the container.
            
            builder.Services.AddSingleton<IMessageBroker>(serviceProvider =>
            {
                var brokerType = brokerConfig.GetValue<BrokerType>("Type");
                var connectionString = brokerConfig.GetValue<string>("ConnectionString");

                return brokerType switch
                {
                    BrokerType.FileSystem => new FileSystemBroker(connectionString),
                    _ => throw new NotSupportedException($"Тип брокера {brokerType} не поддерживается")
                };
            });
            builder.Services.AddSingleton<ILogger, Logger>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.MapControllers();
            app.Use(async (context, next) =>
            {
                Console.WriteLine($"→ {DateTime.UtcNow:HH:mm:ss} {context.Request.Method} {context.Request.Path}");
                await next();
                Console.WriteLine($"← {DateTime.UtcNow:HH:mm:ss} {context.Response.StatusCode}");
            });

            app.Run();
        }
    }
}
