using BarakoCups.Service;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BarakoCups.Data;
using BarakoCups.Models.UserRole;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BarakoCupsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BarakoCupsContext") ?? throw new InvalidOperationException("Connection string 'BarakoCupsContext' not found.")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(option =>
{
    option.Cookie.Name = "BarakoCups.Session";
    //option.IdleTimeout = TimeSpan.FromMinutes(59);
    option.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Account/AccessDenied"; // Ensure this path is correct
 
    
    });


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();



using (var scope = app.Services.CreateScope())
{

    var context = scope.ServiceProvider.GetRequiredService<BarakoCupsContext>();


    // Apply any pending migrations before checking data
    context.Database.Migrate();


    //// Seed Roles if empty  
    //if (!context.Role.Any())
    //{
    //    context.Role.AddRange(
    //       new Role { RoleName = "Admin" },
    //       new Role { RoleName = "Staff" }
    //   );
    //    context.SaveChanges();  // Save Roles  
    //}

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
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSession();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Login}/{action=Index}/{id?}");

    app.Run();
}