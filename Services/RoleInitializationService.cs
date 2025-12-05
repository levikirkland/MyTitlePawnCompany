using Microsoft.AspNetCore.Identity;

namespace MyTitlePawnCompany.Services
{
    public interface IRoleInitializationService
    {
        Task InitializeRolesAsync(RoleManager<IdentityRole> roleManager);
    }

    public class RoleInitializationService : IRoleInitializationService
    {
        private readonly ILogger<RoleInitializationService> _logger;

        public RoleInitializationService(ILogger<RoleInitializationService> logger)
        {
            _logger = logger;
        }

        public async Task InitializeRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[]
            {
                "CompanyAdmin",      // Full access to all company functions
                "StoreManager",      // Manage single store, approve loans
                "Associate",         // All functions except approve
                "ApprovingAssociate" // Associate + approval policy/limit
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Role '{role}' created successfully.");
                    }
                    else
                    {
                        _logger.LogError($"Failed to create role '{role}'.");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Role Permissions Reference
    /// 
    /// CompanyAdmin: 
    ///   - View all stores
    ///   - Create/Edit/Delete stores
    ///   - Manage users and roles
    ///   - Approve all loans (no limit)
    ///   - View company reports
    ///   - Configure company settings
    /// 
    /// StoreManager:
    ///   - Manage assigned store(s)
    ///   - Approve loans in assigned store(s) (no limit)
    ///   - Manage store associates
    ///   - View store reports
    ///   - Cannot access other stores
    /// 
    /// Associate:
    ///   - Create new pawn requests
    ///   - Record payments
    ///   - View loan details
    ///   - View assigned store reports
    ///   - Cannot approve loans
    ///   - Cannot manage users
    /// 
    /// ApprovingAssociate:
    ///   - All Associate permissions
    ///   - Approve loans up to configured limit (e.g., $500)
    ///   - Approval limit set at user level
    ///   - Cannot exceed daily or monthly limits
    /// </summary>
}
