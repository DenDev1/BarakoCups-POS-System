namespace BarakoCups.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int Stock { get; set; }        // will be deducted on checkout
        public bool IsActive { get; set; } = true;

        public enum PaymentMethod
        {
            Cash = 1,
            Credit = 2
        }
    }
}
