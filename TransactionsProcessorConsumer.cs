using AutoMapper;
using Confluent.Kafka;
using Newtonsoft.Json;
using TransactionsProcessor.Data;
using TransactionsProcessor.Entities;
using TransactionsProcessor.Models;

namespace TransactionsProcessor;

public class TransactionsProcessorConsumer : BackgroundService
{
    private readonly ILogger<TransactionsProcessorConsumer> _logger;
    private readonly ConsumerConfig _consumerConfig;
    private readonly string _topic;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMapper _mapper;

    public TransactionsProcessorConsumer(
        ILogger<TransactionsProcessorConsumer> logger,
        ConsumerConfig consumerConfig,
        string topic,
        IServiceProvider serviceProvider,
        IMapper mapper
    )
    {
        _serviceProvider = serviceProvider;
        _mapper = mapper;
        _logger = logger;
        _consumerConfig = consumerConfig;
        _topic = topic;
        _consumerConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
        _consumerConfig.EnableAutoCommit = true;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        consumer.Subscribe(_topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    if (consumeResult != null)
                    {
                        _logger.LogInformation(consumeResult.Message.Value);

                        var tansactionRequest =
                            JsonConvert.DeserializeObject<TransactionRequest>(consumeResult.Message.Value);

                        using var scope = _serviceProvider.CreateAsyncScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        Transaction transaction = _mapper.Map<Transaction>(tansactionRequest);

                        var existingTransaction = dbContext.Transactions
                            .FirstOrDefault(x => x.RequestReference == transaction.RequestReference);

                        if (existingTransaction == null)
                        {
                            dbContext.Transactions.Add(transaction);
                            dbContext.SaveChanges();

                            _logger.LogInformation($"Transaction Response: RequestReference {transaction.RequestReference} has been processed");
                        }
                        else
                        {
                            _logger.LogInformation($"Transaction Response: RequestReference {transaction.RequestReference} already exists");
                        }
                    }
                }
                catch (ConsumeException e)
                {
                    _logger.LogError($"Error occurred: {e.Error.Reason}");
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error occurred: {e}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer stopped.");
        }
        finally
        {
            consumer.Close();
        }
    }
}
