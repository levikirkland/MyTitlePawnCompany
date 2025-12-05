using Microsoft.AspNetCore.Identity;
using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Services
{
    public static class DatabaseSeedService
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "CompanyAdmin", "StoreManager", "Associate", "ApprovingAssociate" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedInitialDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            try
            {
                await SeedRolesAsync(roleManager);
                Console.WriteLine("? Roles seeded");

                // Check if we already have companies - but also check users
                var companyCount = context.Companies.Count();
                var userCount = context.Users.Count();
                Console.WriteLine($"?? Current state: {companyCount} companies, {userCount} users");

                if (companyCount > 0 && userCount > 0)
                {
                    Console.WriteLine("? Database already seeded, skipping...");
                    return;
                }

                // If partial data exists, clear it
                if (companyCount > 0 || userCount > 0)
                {
                    Console.WriteLine("??  Partial data detected, clearing and reseeding...");
                    // Clear existing data
                    context.StoreUsers.RemoveRange(context.StoreUsers);
                    context.Stores.RemoveRange(context.Stores);
                    context.Companies.RemoveRange(context.Companies);
                    await context.SaveChangesAsync();
                    Console.WriteLine("? Cleared existing data");
                }

                Console.WriteLine("?? Seeding database with initial data...");

                // Create default company
                var company = new Company
                {
                    Name = "My Title Pawn Company",
                    Address = "123 Main St",
                    Phone = "(555) 123-4567",
                    Email = "admin@mytitlepawncompany.com",
                    TaxId = "12-3456789",
                    IsActive = true
                };

                await context.Companies.AddAsync(company);
                await context.SaveChangesAsync();
                Console.WriteLine($"? Company created (ID: {company.Id})");

                // Create default store
                var store = new Store
                {
                    CompanyId = company.Id,
                    Name = "Main Store",
                    Address = "123 Main St",
                    Phone = "(555) 123-4567",
                    Email = "main@mytitlepawncompany.com",
                    StoreCode = "STR-001",
                    IsActive = true
                };

                await context.Stores.AddAsync(store);
                await context.SaveChangesAsync();
                Console.WriteLine($"? Store created (ID: {store.Id})");

                // Create default admin user
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@mytitlepawncompany.com",
                    Email = "admin@mytitlepawncompany.com",
                    FirstName = "Admin",
                    LastName = "User",
                    CompanyId = company.Id,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, "AdminPassword123!");
                if (result.Succeeded)
                {
                    Console.WriteLine($"? Admin user created (ID: {adminUser.Id})");
                    
                    await userManager.AddToRoleAsync(adminUser, "CompanyAdmin");
                    Console.WriteLine("? Admin role assigned");

                    // Assign admin to default store
                    var storeUser = new StoreUser
                    {
                        UserId = adminUser.Id,
                        StoreId = store.Id,
                        IsDefault = true
                    };
                    await context.StoreUsers.AddAsync(storeUser);
                    await context.SaveChangesAsync();
                    Console.WriteLine("? Admin assigned to store");
                    
                    Console.WriteLine("\n? DATABASE SEEDING COMPLETED SUCCESSFULLY!");
                    Console.WriteLine("???????????????????????????????????????????");
                    Console.WriteLine("?? Email:    admin@mytitlepawncompany.com");
                    Console.WriteLine("?? Password: AdminPassword123!");
                    Console.WriteLine("???????????????????????????????????????????\n");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    Console.WriteLine($"? Failed to create admin user: {errors}");
                    throw new Exception($"Failed to seed admin user: {errors}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? ERROR DURING SEEDING: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
