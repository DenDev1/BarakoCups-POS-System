namespace BarakoCups.ViewModels
{
    public class CartItemVM
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } = default!;
        public decimal Subtotal => Price * Quantity;
    }

    public class CartVM
    {
        public List<CartItemVM> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.Subtotal);
    }

    public static class SessionKeys
    {
        public const string CART = "BARAKO_CART";
    }

}
