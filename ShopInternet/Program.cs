using Microsoft.EntityFrameworkCore;
using ShopInternet.Data;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using ShopInternet.Interfaces;
using ShopInternet.Utility;
using Microsoft.AspNetCore.Identity;
using ShopInternet;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

#region MSSQL
var connectionStr = builder.Configuration.GetConnectionString("MSSQL") ?? throw new Exception("Connection string not found");
builder.Services.AddDbContext<ShopDbContext>(options => options.UseSqlServer(connectionStr));
#endregion

#region Identity Settings
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ShopDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";
    options.LogoutPath = "/Admin/Account/Logout";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "Site_Identity";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});
#endregion

#region Access Policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PremiumAccess", policy => policy.RequireRole(WC.AdminRole, WC.ManagerRole, WC.PremiumUser));
    options.AddPolicy("AdminAccess", policy => policy.RequireRole(WC.AdminRole));
    options.AddPolicy("RegisterUserAccess", policy => policy.RequireRole(WC.AdminRole, WC.ManagerRole, WC.PremiumUser, WC.CustomerRole));
});
#endregion

builder.Services.AddSession(Options =>
{
    Options.IdleTimeout = TimeSpan.FromMinutes(30);
    Options.Cookie.HttpOnly = true;
    Options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IUploader, FileUploader>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

#region Create Roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { WC.AdminRole, WC.CustomerRole, WC.PremiumUser, WC.ManagerRole };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
#endregion

#region Seed Admin

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    const string adminEmail = "admin@shop.com";
    const string adminPassword = "Admin123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, WC.AdminRole);
        }
    }
    else if (!await userManager.IsInRoleAsync(adminUser, WC.AdminRole))
    {
        await userManager.AddToRoleAsync(adminUser, WC.AdminRole);
    }
}
#endregion


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var supportedCultures = new[] { new CultureInfo("uk-UA") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("uk-UA"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

#region Features
var isAdminEnable = builder.Configuration.GetValue<bool>("Features:EnableAdmin");
if (isAdminEnable)
{
    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
}
#endregion

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();