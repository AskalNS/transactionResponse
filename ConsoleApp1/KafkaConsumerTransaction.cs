using Confluent.Kafka;
using ConsoleApp1.Repository;
using Newtonsoft.Json;
using Transactions.models;
using WebApplication6.Models;

namespace Transactions
{
    class KafkaConsumerTransaction
    {
        private readonly string _topic;
        private readonly ConsumerConfig _config;
        private static TransactionRepository repo = new TransactionRepository("Host=localhost;Port=5432;Database=crawdinvest;Username=postgres;Password=1234;");

        public KafkaConsumerTransaction(string topic, string groupId, string bootstrapServers)
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
            Console.WriteLine($"Processed message: {message}");

            TransactionDTOResponse transactionDTO = JsonConvert.DeserializeObject<TransactionDTOResponse>(message);

            if(transactionDTO.Result == 0)
            {
                return;
            }
            else if(transactionDTO.Result == 1)
            {

                repo.InsertTransaction(transactionDTO);

            }

        }
    }
}
