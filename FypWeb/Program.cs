
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Fyp.DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Fyp.Utility;
using Fyp.Models;
using FypWeb.IService;
using FypWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
/*builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
});*/

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("FypConnectionString")));
builder.Services.AddRazorPages();
builder.Services.AddIdentity<ApplicationUser,IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "AspNetCore.Identity.Application";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(25);
    options.SlidingExpiration = true;
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";

});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(25);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
/*builder.Services.AddScoped<IEmailSender, EmailSender>();*/
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<INotiService, NotiService>();


var app = builder.Build();

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
// Redirect to the login page if not authenticated for any route
/*app.Use(async (context, next) =>
{
    // Check if the user is not authenticated and not already requesting the login page
    if (!context.User.Identity.IsAuthenticated && 
        !context.Request.Path.StartsWithSegments("/Identity/Account/Login") &&
        !context.Request.Path.StartsWithSegments("/Identity/Account/Register")) // Exclude register or any other paths as needed
    {
        // Redirect to the login page
        context.Response.Redirect("/Identity/Account/Login");
    }
    else
    {
        // Proceed with the request
        await next.Invoke();
    }
});*/

app.UseSession();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
/*    pattern: "/{controller=Access}/{action=Login}/{id?}");
*/
pattern: "{area=Admin}/{controller=Home}/{action=Index}/{id?}");

app.Run();
