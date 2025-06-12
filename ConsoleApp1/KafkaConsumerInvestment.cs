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
        private static readonly TransactionRepository repo = new TransactionRepository("Host=localhost;Port=5432;Database=crawdinvest;Username=postgres;Password=1234;");

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
            Console.WriteLine($"[KafkaConsumer] Подписка на топик: {_topic}");
            consumer.Subscribe(_topic);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);
                        Console.WriteLine($"[KafkaConsumer] Получено сообщение: {consumeResult.Message.Value} | Offset: {consumeResult.TopicPartitionOffset}");

                        await ProcessMessageAsync(consumeResult.Message.Value);

                        consumer.Commit(consumeResult);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("[KafkaConsumer] Остановка по токену.");
                        break;
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"[KafkaConsumer] Ошибка получения из Kafka: {e.Error.Reason}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[KafkaConsumer] Общая ошибка: {e.Message}");
                    }
                }
            }
            finally
            {
                Console.WriteLine("[KafkaConsumer] Завершение и закрытие подписки.");
                consumer.Close();
            }
        }

        private static async Task ProcessMessageAsync(string message)
        {
            try
            {
                await Task.Delay(500);
                Console.WriteLine($"[Process] Начата обработка сообщения: {message}");

                InvestmentResponseDTO investmentDTO = JsonConvert.DeserializeObject<InvestmentResponseDTO>(message);

                Console.WriteLine($"[Process] Десериализован DTO: InvestorId={investmentDTO.InvestorId}, OrderId={investmentDTO.OrderId}, Amount={investmentDTO.Amount}, Result={investmentDTO.result}");

                if (investmentDTO.result == 0)
                {
                    Console.WriteLine($"[Process] Оплата неуспешна. Пропуск сохранения.");
                    return;
                }

                if (investmentDTO.result == 1)
                {
                    Console.WriteLine($"[Process] Оплата успешна. Сохраняем инвестицию...");

                    repo.InsertInvestment(investmentDTO);
                    Console.WriteLine($"[Process] Инвестиция добавлена в БД.");

                    int amount = Convert.ToInt32(investmentDTO.Amount);
                    repo.UpdateOrderCurrentAmount(investmentDTO.OrderId, amount);
                    Console.WriteLine($"[Process] Обновлён текущий объём инвестиций по заказу OrderId={investmentDTO.OrderId} на сумму {amount}");
                }
                else
                {
                    Console.WriteLine($"[Process] Неизвестный статус result={investmentDTO.result}. Ничего не делаем.");
                }
            }
            catch (JsonException je)
            {
                Console.WriteLine($"[Process] Ошибка при разборе JSON: {je.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Process] Ошибка обработки сообщения: {ex}");
            }
        }
    }
}
