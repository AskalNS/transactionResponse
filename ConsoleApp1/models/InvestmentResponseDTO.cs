using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transactions.models
{
    public class InvestmentResponseDTO
    {
        public int InvestorId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int result;
    }
}
