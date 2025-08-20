using BarakoCups.ViewModels;
using System.Text.Json;

namespace BarakoCups.Service
{
    public class CartService
    {
        private readonly IHttpContextAccessor _ctx;
        public CartService(IHttpContextAccessor ctx) => _ctx = ctx;

        public CartVM Get()
        {
            var str = _ctx.HttpContext!.Session.GetString(SessionKeys.CART);
            return string.IsNullOrEmpty(str) ? new CartVM() : JsonSerializer.Deserialize<CartVM>(str)!;
        }

        public void Save(CartVM cart)
        {
            var str = JsonSerializer.Serialize(cart);
            _ctx.HttpContext!.Session.SetString(SessionKeys.CART, str);
        }

        public void Clear() => _ctx.HttpContext!.Session.Remove(SessionKeys.CART);
    }
}
