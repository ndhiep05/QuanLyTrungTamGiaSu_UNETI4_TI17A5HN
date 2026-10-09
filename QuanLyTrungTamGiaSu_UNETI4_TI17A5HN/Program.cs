<<<<<<< Updated upstream
﻿using Microsoft.EntityFrameworkCore;
=======

using Microsoft.EntityFrameworkCore;
>>>>>>> Stashed changes
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Data;

var builder = WebApplication.CreateBuilder(args);

<<<<<<< Updated upstream
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDistributedMemoryCache();
=======
// MVC
builder.Services.AddControllersWithViews();

// SQL Server + Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Session
builder.Services.AddDistributedMemoryCache();

>>>>>>> Stashed changes
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
<<<<<<< Updated upstream
=======
    options.Cookie.SecurePolicy =
        CookieSecurePolicy.Always;
>>>>>>> Stashed changes
});

var app = builder.Build();

<<<<<<< Updated upstream
=======
// Error handling
>>>>>>> Stashed changes
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

<<<<<<< Updated upstream
app.UseSession();        // phải nằm sau UseRouting và trước UseAuthorization
=======
// Session must be available before Controllers use it
app.UseSession();

// Authorization
>>>>>>> Stashed changes
app.UseAuthorization();

// Static assets
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();