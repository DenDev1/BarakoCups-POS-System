using static BarakoCups.Models.Product;

namespace BarakoCups.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Total { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
