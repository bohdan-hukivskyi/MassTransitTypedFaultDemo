using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using TypedFaultDemo;

await using RabbitMqContainer rabbitMqContainer = new RabbitMqBuilder().Build();
await using MsSqlContainer msSqlContainer = new MsSqlBuilder().Build();

await rabbitMqContainer.StartAsync();
await msSqlContainer.StartAsync();

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddDbContext<TestDbContext>(options => 
    options.UseSqlServer(msSqlContainer.GetConnectionString())
);

builder.Services.Configure<RabbitMqTransportOptions>(options =>
{
    var cs = rabbitMqContainer.GetConnectionString();
    var match = Regex.Match(cs, "amqp://(?<user>[^:]+):(?<password>[^@]+)@(?<host>[^:]+):(?<port>[0-9]+)/");
    options.Host = match.Groups["host"].Value;
    options.Port = ushort.Parse(match.Groups["port"].Value);
    options.User = match.Groups["user"].Value;
    options.Pass = match.Groups["password"].Value;
});

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, config) =>
    {
        config.ConfigureEndpoints(context);
    });

    configurator.AddEntityFrameworkOutbox<TestDbContext>(options =>
    {
        options.UseSqlServer();
    });

    configurator.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        cfg.UseEntityFrameworkOutbox<TestDbContext>(context);
    });

    configurator.AddConsumer<TestMessageConsumer>();

    configurator.AddConsumer<TestFaultTypedMessageConsumer>();

    configurator.AddConsumer<TestFaultNotTypedMessageConsumer>();
});

var host = builder.Build();

await host.MigrateDb<TestDbContext>();

await host.StartAsync();

await Task.Delay(TimeSpan.FromSeconds(3));

var publisher = host.Services.GetRequiredService<IBus>();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

var testEntityId = Guid.NewGuid();

logger.LogInformation("Publish correct message");

await publisher.Publish(new TestMessage()
{
    TestId = testEntityId,
    ThrowInConsumer = false
});

await Task.Delay(TimeSpan.FromSeconds(2));

logger.LogInformation("Publish message with in consumer error");

await publisher.Publish(new TestMessage()
{
    TestId = testEntityId,
    ThrowInConsumer = true
});

await Task.Delay(TimeSpan.FromSeconds(2));

logger.LogInformation("Publish message with out consumer error (unique key violation)");

await publisher.Publish(new TestMessage()
{
    TestId = testEntityId,
    ThrowInConsumer = false
});

await Task.Delay(TimeSpan.FromSeconds(2));

Console.WriteLine("Press any key to exit");

Console.ReadKey();

await host.StopAsync();