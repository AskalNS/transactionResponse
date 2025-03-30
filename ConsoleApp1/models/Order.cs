using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.models
{
    [Table("Order")]
    public class Order
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("iin")]
        public int BusinessId { get; set; }

        [Column("isActive")]
        public int IsActive { get; set; }

        [Column("targetAmount")]
        public int TargetAmount { get; set; }

        [Column("currentAmount")]
        public int CurrentAmount { get; set; }


        [Column("target")]
        public string Target { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("plan")]
        public string Plan { get; set; }

        [Column("isChecked")]
        public int IsChecked { get; set; }

        [Column("dateOfOrder")]
        public DateTimeOffset DateOfOrder { get; set; }

        [Column("dueDate")]
        public DateTimeOffset DueDate { get; set; }
    }
}
