using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transactions.models
{

    [Table("Transaction")]
    public class Transaction
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("investorId")]
        public int InvestorId { get; set; }

        [Column("orderId")]
        public int OrderId { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("trasactionType")]
        public int TrasactionType { get; set; }

        [Column("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

    }
}
