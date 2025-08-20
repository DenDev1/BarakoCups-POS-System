using BarakoCups.Data;
using BarakoCups.Models;
using BarakoCups.Service;
using BarakoCups.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class POSController : Controller
{
    private readonly BarakoCupsContext _db;
    private readonly CartService _cart;

    public POSController(BarakoCupsContext db, CartService cart)
    {
        _db = db;
        _cart = cart;
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
