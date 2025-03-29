// See https://aka.ms/new-console-template for more information
using ConsoleApp1.Repository;
using Microsoft.Extensions.Configuration;
using Transactions;
using WebApplication6.Models;

Console.WriteLine("Hello, World!");


var repo = new TransactionRepository("Host=localhost;Port=5432;Database=crawdinvest;Username=postgres;Password=1234;");

//repo.InsertTransaction(new TransactionDTO
//{
//    InvestorId = 1,
//    OrderId = 123,
//    Amount = 100.50m,
//    CreatedAt = DateTimeOffset.UtcNow,
//    TrasactionType = 1010
//});


//var cts = new CancellationTokenSource();
//Console.CancelKeyPress += (_, e) => {
//    e.Cancel = true;
//    cts.Cancel();
//};

//var consumer1 = new KafkaConsumerInvestment("InvestorPaymentResponse", "consumer-group-1", "localhost:9092");
//var consumer2 = new KafkaConsumerRefill("BusinessRefillResponse", "consumer-group-1", "localhost:9092");
//var consumer3 = new KafkaConsumerTransaction("InvestorTransactionResponse", "consumer-group-1", "localhost:9092");

//await Task.WhenAll(
//    consumer3.StartConsuming(cts.Token)
//);