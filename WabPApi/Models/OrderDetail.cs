using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WabPApi.Models
{
    [Table("wabpOrderDetails")]
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        public int BookId { get; set; }
        public virtual Book Book { get; set; }
        public virtual Order Order { get; set; }
        public int? PaymentStatus { get; set; }
        public int? DeliveryStatus { get; set; } = 0;
        public string TransactionReference { get; set; }
    }

    public class OrderDTO
    {
        public int OrderId { get; set; }
        public string Username { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int BookId { get; set; }
        public string Title { get; set; }
        public string PicPath { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
