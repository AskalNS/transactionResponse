using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using ConsoleApp1.Repository;
using Newtonsoft.Json;
using Transactions.models;

namespace Transactions
{

    public class KafkaConsumerInvestment
    {
        private readonly string _topic;
        private readonly ConsumerConfig _config;
        private static TransactionRepository repo = new TransactionRepository("Host=localhost;Port=5432;Database=crawdinvest;Username=postgres;Password=1234;");

        public KafkaConsumerInvestment(string topic, string groupId, string bootstrapServers)
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
            Console.WriteLine("333333333333333333333333333");
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

            InvestmentResponseDTO investmentDTO = JsonConvert.DeserializeObject<InvestmentResponseDTO>(message);

            if(investmentDTO.result == 0)
            {
                return;
            }
            else if(investmentDTO.result == 1)
            {
                repo.InsertInvestment(investmentDTO);
                repo.UpdateOrderCurrentAmount(investmentDTO.OrderId, Convert.ToInt32(investmentDTO.Amount));
            }
        }
    }
}