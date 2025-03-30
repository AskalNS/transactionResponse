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

        public void InsertRefill(RefillDTOResponse refillDTO)
        {
            using var db = CreateConnection();
            string sql = @"INSERT INTO dbo.""Refill"" (""businessId"", ""createdAt"", ""orderId"", ""amount"") 
                   VALUES (@businessId, @createdAt, @orderId, @amount);";

            db.Execute(sql, new
            {
                refillDTO.BusinessId,
                CreatedAt = refillDTO.CreatedAt.UtcDateTime,
                refillDTO.OrderId,
                refillDTO.Amount
            });
        }

        public void InsertInvestment(InvestmentResponseDTO investmentDTO)
        {
            using var db = CreateConnection();
            string sql = @"INSERT INTO dbo.""Investing"" (""investorId"", ""orderId"", ""amount"", ""createdAt"") 
                   VALUES (@investorId, @orderId, @amount, @createdAt);";

            try
            {
                db.Execute(sql, new
                {
                    investmentDTO.InvestorId,
                    investmentDTO.OrderId,
                    investmentDTO.Amount,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
            catch(Exception e)
            {

            }
        }
        public List<Investing> GetInvestmentsByOrderId(int orderId)
        {
            using var db = CreateConnection();
            string sql = @"SELECT ""id"", ""investorId"", ""orderId"", ""amount"", ""createdAt"" 
                   FROM dbo.""Investing"" 
                   WHERE ""orderId"" = @orderId;";

            return db.Query<Investing>(sql, new { orderId }).ToList();
        }

        public void UpdateOrderCurrentAmount(int orderId, int amountToAdd)
        {
            using var db = CreateConnection();

            string sql = @"UPDATE dbo.""Order"" 
                   SET ""currentAmount"" = ""currentAmount"" + @amountToAdd 
                   WHERE ""id"" = @orderId;";

            db.Execute(sql, new { orderId, amountToAdd });
        }
    }
}
