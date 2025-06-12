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
        private static readonly TransactionRepository repo = new TransactionRepository("Host=localhost;Port=5432;Database=crawdinvest;Username=postgres;Password=1234;");

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
            Console.WriteLine($"[KafkaConsumerRefill] Подписка на топик: {_topic}");
            consumer.Subscribe(_topic);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);
                        Console.WriteLine($"[KafkaConsumerRefill] Получено сообщение: {consumeResult.Message.Value} | Offset: {consumeResult.TopicPartitionOffset}");

                        await ProcessMessageAsync(consumeResult.Message.Value);

                        consumer.Commit(consumeResult);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("[KafkaConsumerRefill] Остановка по токену.");
                        break;
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"[KafkaConsumerRefill] Ошибка Kafka: {e.Error.Reason}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[KafkaConsumerRefill] Ошибка обработки: {e}");
                    }
                }
            }
            finally
            {
                Console.WriteLine("[KafkaConsumerRefill] Завершение работы и отписка.");
                consumer.Close();
            }
        }

        private static async Task ProcessMessageAsync(string message)
        {
            try
            {
                await Task.Delay(500);
                Console.WriteLine($"[Process] Начата обработка сообщения: {message}");

                var refillDTO = JsonConvert.DeserializeObject<RefillDTOResponse>(message);

                Console.WriteLine($"[Process] Десериализован DTO: OrderId={refillDTO.OrderId}, Amount={refillDTO.Amount}, Result={refillDTO.Result}");

                if (refillDTO.Result == 0)
                {
                    Console.WriteLine("[Process] Пополнение отклонено (Result = 0). Пропуск.");
                    return;
                }

                if (refillDTO.Result == 1)
                {
                    Console.WriteLine("[Process] Пополнение подтверждено. Сохраняем пополнение в БД.");
                    repo.InsertRefill(refillDTO);

                    List<Investing> investments = repo.GetInvestmentsByOrderId(refillDTO.OrderId);
                    Console.WriteLine($"[Process] Найдено инвестиций по заказу: {investments.Count}");

                    decimal totalInvestSum = 0;
                    foreach (var investing in investments)
                        totalInvestSum += investing.Amount;

                    if (totalInvestSum == 0)
                    {
                        Console.WriteLine("[Process] Сумма инвестиций = 0. Деление невозможно. Пропуск.");
                        return;
                    }

                    foreach (var investing in investments)
                    {
                        var proportionalAmount = refillDTO.Amount / totalInvestSum * investing.Amount;

                        var transaction = new TransactionDTOResponse
                        {
                            InvestorId = investing.InvestorId,
                            OrderId = refillDTO.OrderId,
                            Amount = proportionalAmount,
                            TrasactionType = 1010,
                            CreatedAt = DateTimeOffset.Now,
                            Result = 1
                        };

                        repo.InsertTransaction(transaction);
                        Console.WriteLine($"[Process] Добавлена транзакция для инвестора {investing.InvestorId}: сумма = {proportionalAmount}");
                    }
                }
                else
                {
                    Console.WriteLine($"[Process] Неизвестный статус Result = {refillDTO.Result}. Пропуск.");
                }
            }
            catch (JsonException je)
            {
                Console.WriteLine($"[Process] Ошибка JSON-десериализации: {je.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Process] Ошибка обработки сообщения: {ex}");
            }
        }
    }
}
