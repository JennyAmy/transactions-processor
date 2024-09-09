using AutoMapper;
using Confluent.Kafka;
using Newtonsoft.Json;
using TransactionsProcessor.Data;
using TransactionsProcessor.Models;

namespace TransactionsProcessor;

public class TransactionsProcessorProducer : BackgroundService
{
    private readonly ILogger<TransactionsProcessorProducer> _logger;
    private readonly string _topic;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMapper _mapper;
    private readonly IProducer<string, string> _producer;

    public TransactionsProcessorProducer(
        ILogger<TransactionsProcessorProducer> logger,
        ProducerConfig producerConfig,
        string topic,
        IServiceProvider serviceProvider,
        IMapper mapper
    )
    {
        _serviceProvider = serviceProvider;
        _mapper = mapper;
        _logger = logger;
        _topic = topic;

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateAsyncScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var transactions = dbContext.Transactions
                        .Where(x => !x.Processed);

                    if (transactions.Any())
                    {
                        List<TransactionRequest> transactionRequests = _mapper.Map<List<TransactionRequest>>(transactions);

                        var messageToPublish = new Message<string, string>
                        {
                            Value = JsonConvert.SerializeObject(transactions)
                        };

                        var deliveryResult = await _producer.ProduceAsync(_topic, messageToPublish);

                        _logger.LogInformation($"Successfully published {transactionRequests.Count} transactions to {deliveryResult.TopicPartitionOffset}");
                    }
                    else
                    {
                        _logger.LogInformation($"There are currently no transactions to process");
                    }

                }
                catch (ProduceException<string, string> e)
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
            _logger.LogInformation("Kafka producer stopped.");
        }
    }
}
