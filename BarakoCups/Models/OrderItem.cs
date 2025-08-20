namespace BarakoCups.Models
{
    public class OrderItem
    {
        public int OrderITemId { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = default!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => UnitPrice * Quantity;
    }
}
