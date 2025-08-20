using BarakoCups.Service;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BarakoCups.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BarakoCupsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BarakoCupsContext") ?? throw new InvalidOperationException("Connection string 'BarakoCupsContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
// Our cart helper
builder.Services.AddScoped<CartService>();


builder.Services.AddHttpContextAccessor();

var app = builder.Build();
// Session for cart




// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();           // <-- important
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
