using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;
using MyTitlePawnCompany.Services;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.SignIn.RequireConfirmedEmail = false;
});

// Configure authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add repositories and services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITitlePawnService, TitlePawnService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IPaymentScheduleService, PaymentScheduleService>();
builder.Services.AddScoped<IRoleInitializationService, RoleInitializationService>();
builder.Services.AddScoped<IRateRecommendationService, RateRecommendationService>();
builder.Services.AddScoped<IFeeService, FeeService>();

// Add Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        Console.WriteLine("?? Applying migrations...");
        db.Database.Migrate();
        Console.WriteLine("? Migrations applied");

        Console.WriteLine("?? Starting database seeding...");
        await DatabaseSeedService.SeedInitialDataAsync(db, userManager, roleManager);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Error during startup: {ex.Message}");
        Console.WriteLine($"? Stack trace: {ex.StackTrace}");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
