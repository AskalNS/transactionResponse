using Confluent.Kafka;
using ConsoleApp1.Repository;
using Newtonsoft.Json;
using Transactions.models;
using WebApplication6.Models;

namespace Transactions
{
    class KafkaConsumerRefill
    {
        private readonly string _topic;
        private readonly ConsumerConfig _config;
        private static TransactionRepository repo = new TransactionRepository("Host=localhost;Port=5432;Database=crawdinvest;Username=postgres;Password=1234;");

        public KafkaConsumerRefill(string topic, string groupId, string bootstrapServers)
        {
            _topic = topic;
            _config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };
        }

        public async Task StartConsuming(CancellationToken cancellationToken)
        {
            await Task.Yield();

            using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
            Console.WriteLine("22222222222222222222222222222");
            consumer.Subscribe(_topic);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);
                        Console.WriteLine($"Received message: {consumeResult.Message.Value} at {consumeResult.TopicPartitionOffset}");

                        await ProcessMessageAsync(consumeResult.Message.Value);

                        consumer.Commit(consumeResult);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Kafka consume error: {e.Error.Reason}");
                    }
                }
            }
            finally
            {
                consumer.Close();
            }
        }

        private static async Task ProcessMessageAsync(string message)
        {
            await Task.Delay(500);
            Console.WriteLine($"Processed message: {message}");

            RefillDTOResponse refillDTO = JsonConvert.DeserializeObject<RefillDTOResponse>(message);

            if(refillDTO.Result == 0)
            {
                return;
            }
            else if(refillDTO.Result == 1)
            {
                repo.InsertRefill(refillDTO);

                List<Investing> investments = repo.GetInvestmentsByOrderId(refillDTO.OrderId);

                decimal totalInvestSum = 0;
                foreach (var investing in investments)
                {
                    totalInvestSum += investing.Amount;
                }

                foreach (var investing in investments)
                {
                    var transaction = new TransactionDTOResponse()
                    {
                        InvestorId = investing.InvestorId,
                        OrderId = refillDTO.OrderId,
                        Amount = refillDTO.Amount / totalInvestSum * investing.Amount,
                        TrasactionType = 1010,
                        CreatedAt = DateTimeOffset.Now,
                        Result = 1
                    };
                    repo.InsertTransaction(transaction);

                }
            }
        }
    }
}
