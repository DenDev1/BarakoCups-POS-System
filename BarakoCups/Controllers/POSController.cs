using BarakoCups.Data;
using BarakoCups.Models;
using BarakoCups.Service;
using BarakoCups.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using static BarakoCups.Models.Product;

public class POSController : Controller
{
    private readonly BarakoCupsContext _db;
    private readonly CartService _cart;

    public POSController(BarakoCupsContext db, CartService cart)
    {
        _db = db;
        _cart = cart;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(string paymentMethod)
    {
        if (paymentMethod == "GCash")
        {
            return await RedirectToGCash();
        }
            // ✅ Get cart from your cart service or session
            var cart = _cart.Get(); // This should return a cart object with Items: List<CartItem>
        if (cart == null || !cart.Items.Any())
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        // ✅ Validate stock availability
        var productIds = cart.Items.Select(i => i.ProductId).ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.ProductId))
            .ToListAsync();

        foreach (var item in cart.Items)
        {
            var product = products.First(p => p.ProductId == item.ProductId);
            if (product.Stock < item.Quantity)
            {
                TempData["Error"] = $"{product.Name} has only {product.Stock} left in stock.";
                return RedirectToAction("Checkout");
            }
        }

        // ✅ Build the order
        var order = new Order
        {
            CreatedAt = DateTime.Now,
            PaymentMethod = Enum.TryParse<PaymentMethod>(paymentMethod, true, out var parsed) ? parsed : PaymentMethod.Cash,
            Items = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.Price
            }).ToList()
        };

        order.Total = order.Items.Sum(x => x.UnitPrice * x.Quantity);

        // ✅ Deduct stock
        foreach (var item in cart.Items)
        {
            var product = products.First(p => p.ProductId == item.ProductId);
            product.Stock -= item.Quantity;
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // ✅ Clear the cart
        _cart.Clear();

        // ✅ Redirect to receipt view
        return RedirectToAction("Receipt", new { id = order.OrderId });
    }

    private async Task<IActionResult> RedirectToGCash()
    {
        var client = new HttpClient();

        // 🔐 Replace with your own secret key
        var secretKey = "pk_test_zFW8Tvu7jvE9sxFhF7RyTNVz";

        var byteArray = Encoding.ASCII.GetBytes($"{secretKey}:");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var payload = new
        {
            data = new
            {
                attributes = new
                {
                    amount = 10000, // ₱100.00 = 10000 centavos
                    redirect = new
                    {
                        success = Url.Action("Success", "Checkout", null, Request.Scheme),
                        failed = Url.Action("Failed", "Checkout", null, Request.Scheme)
                    },
                    type = "gcash",
                    currency = "PHP"
                }
            }
        };

        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.paymongo.com/v1/sources", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Payment failed to initialize.";
            return View("Failed");
        }

        dynamic result = JsonConvert.DeserializeObject(responseContent);
        string redirectUrl = result.data.attributes.redirect.checkout_url;

        return Redirect(redirectUrl);
    }

    // 3.1 Catalog + Cart
    public async Task<IActionResult> Index()
    {
        var products = await _db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
        ViewBag.Products = products;
        return View(_cart.Get());
    }

    // 3.2 Add to cart
    [HttpPost]
    public async Task<IActionResult> Add(int productId, int qty = 1)
    {
        if (qty < 1) qty = 1;

        var p = await _db.Products.FindAsync(productId);
        if (p is null || !p.IsActive) return NotFound();

        var cart = _cart.Get();
        var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is null)
        {
            cart.Items.Add(new CartItemVM
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Price = p.Price,
                Quantity = qty,
                ImageUrl = p.ImageUrl
            });
        }
        else
        {
            existing.Quantity += qty;
        }

        _cart.Save(cart);
        return RedirectToAction(nameof(Index));
    }

    // 3.3 Update quantity
    [HttpPost]
    public IActionResult UpdateQty(int productId, int qty)
    {
        var cart = _cart.Get();
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            if (qty <= 0) cart.Items.Remove(item);
            else item.Quantity = qty;
            _cart.Save(cart);
        }
        return RedirectToAction(nameof(Index));
    }

    // 3.4 Remove item
    [HttpPost]
    public IActionResult Remove(int productId)
    {
        var cart = _cart.Get();
        cart.Items.RemoveAll(i => i.ProductId == productId);
        _cart.Save(cart);
        return RedirectToAction(nameof(Index));
    }

    // 3.5 Empty cart
    [HttpPost]
    public IActionResult Empty()
    {
        _cart.Clear();
        return RedirectToAction(nameof(Index));
    }

    // 3.6 Checkout (GET shows payment form; POST writes order, deducts stock, clears cart)
    [HttpGet]
    public IActionResult Checkout() => View(new CheckoutVM());

    [HttpPost]
    public async Task<IActionResult> Checkout(CheckoutVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var cart = _cart.Get();
        if (!cart.Items.Any())
        {
            ModelState.AddModelError("", "Cart is empty.");
            return View(vm);
        }

        // Validate stock first
        var productIds = cart.Items.Select(i => i.ProductId).ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.ProductId)).ToListAsync();

        foreach (var i in cart.Items)
        {
            var dbProd = products.First(p => p.ProductId == i.ProductId);
            if (dbProd.Stock < i.Quantity)
            {
                ModelState.AddModelError("", $"{dbProd.Name} has only {dbProd.Stock} left.");
                return View(vm);
            }
        }

        // Create order
        var order = new Order
        {
            PaymentMethod = vm.PaymentMethod,
            Items = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.Price
            }).ToList()
        };
        order.Total = order.Items.Sum(x => x.UnitPrice * x.Quantity);

        // Deduct stock
        foreach (var i in cart.Items)
        {
            var dbProd = products.First(p => p.ProductId == i.ProductId);
            dbProd.Stock -= i.Quantity;
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // Clear cart, redirect to printable receipt
        _cart.Clear();
        return RedirectToAction(nameof(Receipt), new { id = order.OrderId });
    }

    // 3.7 Receipt (print-friendly)
    public async Task<IActionResult> Receipt(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order is null) return NotFound();
        return View(order);
    }
}
