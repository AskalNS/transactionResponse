using System;
using System.Data;
using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using WebApplication6.Models;
using Transactions.models;

namespace ConsoleApp1.Repository
{

    public class TransactionRepository
    {
        private readonly string _connectionString;

        public TransactionRepository(string url)
        {
            _connectionString = url;
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public void InsertTransaction(TransactionDTOResponse transactionDTO)
        {
            using var db = CreateConnection();
            string sql = @"INSERT INTO dbo.""Transaction"" (""investorId"", ""orderId"", ""amount"", ""createdAt"", ""trasactionType"") 
               VALUES (@investorId, @orderId, @amount, @createdAt, @trasactionType);";



            db.Execute(sql, new
            {
                transactionDTO.InvestorId,
                transactionDTO.OrderId,
                transactionDTO.Amount,
                CreatedAt = transactionDTO.CreatedAt.UtcDateTime,
                transactionDTO.TrasactionType
            });
        }

        public void InsertRefill(RefillDTO refillDTO)
        {
            using var db = CreateConnection();
            string sql = @"INSERT INTO dbo.""Refill"" (""businessId"", ""createdAt"", ""orderId"", ""amount"") 
                   VALUES (@businessId, @createdAt, @orderId, @amount);";

            db.Execute(sql, new
            {
                refillDTO.BusinessId,
                CreatedAt = refillDTO.CreatedAt.UtcDateTime,
                refillDTO.OrderId,
                refillDTO.amount
            });
        }

        public void InsertInvestment(InvestmentDTO investmentDTO)
        {
            using var db = CreateConnection();
            string sql = @"INSERT INTO dbo.""Investing"" (""investorId"", ""businessId"", ""amount"", ""createdAt"") 
                   VALUES (@investorId, @businessId, @amount, @createdAt);";

            db.Execute(sql, new
            {
                investmentDTO.InvestorId,
                investmentDTO.BusinessId,
                investmentDTO.amount,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

    }
}
