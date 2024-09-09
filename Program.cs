using Confluent.Kafka;
using Microsoft.AspNetCore.Builder;
using Serilog;
using StackExchange.Redis;
using TransactionsProcessor.Models;
using TransactionsProcessor.Mappings;
using TransactionsProcessor.Data;
using Microsoft.EntityFrameworkCore;
using TransactionsProcessor;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (HostBuilderContext context, IServiceProvider serviceProvider, LoggerConfiguration config) =>
        config.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(serviceProvider)
);

builder.Services.Configure<HostOptions>(x =>
{
    x.ServicesStartConcurrently = true;
    x.ServicesStopConcurrently = false;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddSingleton<ConnectionMultiplexer>(serviceProvider =>
{
    return ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisUrl"));
});

builder.Services.AddAutoMapper(options =>
{
    options.AddProfile<MappingProfile>();
});

var kafkaSettings = builder.Configuration.GetSection("KafkaSettings").Get<KafkaSettings>();

builder.Services.AddHostedService<TransactionsProcessorConsumer>();

var consumerConfig = new ConsumerConfig
{
    BootstrapServers = kafkaSettings!.BootstrapServers,
    GroupId = kafkaSettings.GroupId,
    AutoOffsetReset = AutoOffsetReset.Earliest
};
builder.Services.AddSingleton(consumerConfig);

var producerConfig = new ProducerConfig
{
    BootstrapServers = kafkaSettings!.BootstrapServers,
};

builder.Services.AddSingleton(producerConfig);

var topic = kafkaSettings.Topic;

builder.Services.AddSingleton(topic);
builder.Services.AddWorkerServices();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.Run();
