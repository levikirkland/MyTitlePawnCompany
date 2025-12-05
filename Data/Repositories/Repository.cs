using Microsoft.EntityFrameworkCore;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task SaveChangesAsync();
    }

    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate)
        {
            return await Task.FromResult(_dbSet.Where(predicate).ToList());
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public virtual async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

    // Specialized repositories
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetCustomerWithVehiclesAsync(int customerId);
        Task<IEnumerable<Customer>> GetCompanyCustomersAsync(int companyId);
    }

    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Customer?> GetCustomerWithVehiclesAsync(int customerId)
        {
            return await _dbSet.Include(c => c.Vehicles).FirstOrDefaultAsync(c => c.Id == customerId);
        }

        public async Task<IEnumerable<Customer>> GetCompanyCustomersAsync(int companyId)
        {
            return await _dbSet.Where(c => c.CompanyId == companyId).ToListAsync();
        }
    }

    public interface ICompanyRepository : IRepository<Company>
    {
    }

    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(ApplicationDbContext context) : base(context) { }
    }

    public interface IVehicleRepository : IRepository<Vehicle>
    {
        Task<Vehicle?> GetVehicleWithTitlePawnsAsync(int vehicleId);
        Task<IEnumerable<Vehicle>> GetCustomerVehiclesAsync(int customerId);
    }

    public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Vehicle?> GetVehicleWithTitlePawnsAsync(int vehicleId)
        {
            return await _dbSet.Include(v => v.TitlePawns).FirstOrDefaultAsync(v => v.Id == vehicleId);
        }

        public async Task<IEnumerable<Vehicle>> GetCustomerVehiclesAsync(int customerId)
        {
            return await _dbSet.Where(v => v.CustomerId == customerId).ToListAsync();
        }
    }

    public interface ITitlePawnRepository : IRepository<TitlePawn>
    {
        Task<TitlePawn?> GetTitlePawnWithPaymentsAsync(int titlePawnId);
        Task<IEnumerable<TitlePawn>> GetCompanyTitlePawnsAsync(int companyId);
        Task<IEnumerable<TitlePawn>> GetActiveLoansAsync(int companyId);
        Task<IEnumerable<TitlePawn>> GetDefaultedLoansAsync(int companyId);
    }

    public class TitlePawnRepository : Repository<TitlePawn>, ITitlePawnRepository
    {
        public TitlePawnRepository(ApplicationDbContext context) : base(context) { }

        public async Task<TitlePawn?> GetTitlePawnWithPaymentsAsync(int titlePawnId)
        {
            return await _dbSet
                .Include(tp => tp.Payments)
                .Include(tp => tp.Fees)
                .Include(tp => tp.Vehicle)
                .FirstOrDefaultAsync(tp => tp.Id == titlePawnId);
        }

        public async Task<IEnumerable<TitlePawn>> GetCompanyTitlePawnsAsync(int companyId)
        {
            return await _dbSet
                .Include(tp => tp.Vehicle)
                    .ThenInclude(v => v.Customer)
                .Where(tp => tp.CompanyId == companyId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TitlePawn>> GetActiveLoansAsync(int companyId)
        {
            return await _dbSet.Where(tp => tp.CompanyId == companyId && tp.Status == "Active").ToListAsync();
        }

        public async Task<IEnumerable<TitlePawn>> GetDefaultedLoansAsync(int companyId)
        {
            return await _dbSet.Where(tp => tp.CompanyId == companyId && tp.Status == "Defaulted").ToListAsync();
        }
    }

    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetTitlePawnPaymentsAsync(int titlePawnId);
        Task<IEnumerable<Payment>> GetCompanyPaymentsAsync(int companyId);
    }

    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Payment>> GetTitlePawnPaymentsAsync(int titlePawnId)
        {
            return await _dbSet.Where(p => p.TitlePawnId == titlePawnId).OrderByDescending(p => p.PaymentDate).ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetCompanyPaymentsAsync(int companyId)
        {
            return await _dbSet.Where(p => p.CompanyId == companyId).OrderByDescending(p => p.PaymentDate).ToListAsync();
        }
    }

    public interface ICustomerReferenceRepository : IRepository<CustomerReference>
    {
        Task<IEnumerable<CustomerReference>> GetCustomerReferencesAsync(int customerId);
    }

    public class CustomerReferenceRepository : Repository<CustomerReference>, ICustomerReferenceRepository
    {
        public CustomerReferenceRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<CustomerReference>> GetCustomerReferencesAsync(int customerId)
        {
            return await _dbSet.Where(cr => cr.CustomerId == customerId).OrderByDescending(cr => cr.CreatedDate).ToListAsync();
        }
    }

    public interface IVendorRepository : IRepository<Vendor>
    {
        Task<IEnumerable<Vendor>> GetCompanyVendorsAsync(int companyId);
        Task<IEnumerable<Vendor>> GetVendorsByTypeAsync(int companyId, string vendorType);
    }

    public class VendorRepository : Repository<Vendor>, IVendorRepository
    {
        public VendorRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Vendor>> GetCompanyVendorsAsync(int companyId)
        {
            return await _dbSet.Where(v => v.CompanyId == companyId && v.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Vendor>> GetVendorsByTypeAsync(int companyId, string vendorType)
        {
            return await _dbSet.Where(v => v.CompanyId == companyId && v.VendorType == vendorType && v.IsActive).ToListAsync();
        }
    }

    public interface IReportRepository : IRepository<Report>
    {
        Task<IEnumerable<Report>> GetCompanyReportsAsync(int companyId);
    }

    public class ReportRepository : Repository<Report>, IReportRepository
    {
        public ReportRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Report>> GetCompanyReportsAsync(int companyId)
        {
            return await _dbSet.Where(r => r.CompanyId == companyId).OrderByDescending(r => r.ReportDate).ToListAsync();
        }
    }

    public interface IStoreRepository : IRepository<Store>
    {
        Task<IEnumerable<Store>> GetCompanyStoresAsync(int companyId);
    }

    public class StoreRepository : Repository<Store>, IStoreRepository
    {
        public StoreRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Store>> GetCompanyStoresAsync(int companyId)
        {
            return await _dbSet.Where(s => s.CompanyId == companyId && s.IsActive).OrderBy(s => s.Name).ToListAsync();
        }
    }

    public interface IStoreUserRepository : IRepository<StoreUser>
    {
        Task<StoreUser?> GetDefaultStoreForUserAsync(string userId);
    }

    public class StoreUserRepository : Repository<StoreUser>, IStoreUserRepository
    {
        public StoreUserRepository(ApplicationDbContext context) : base(context) { }

        public async Task<StoreUser?> GetDefaultStoreForUserAsync(string userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(su => su.UserId == userId && su.IsDefault);
        }
    }

    public interface IInterestRateTierRepository : IRepository<InterestRateTier>
    {
        Task<IEnumerable<InterestRateTier>> GetStoreRateTiersAsync(int storeId);
        Task<InterestRateTier?> GetApplicableTierAsync(int storeId, decimal principalAmount);
    }

    public class InterestRateTierRepository : Repository<InterestRateTier>, IInterestRateTierRepository
    {
        public InterestRateTierRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<InterestRateTier>> GetStoreRateTiersAsync(int storeId)
        {
            return await _dbSet
                .Where(t => t.StoreId == storeId && t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();
        }

        public async Task<InterestRateTier?> GetApplicableTierAsync(int storeId, decimal principalAmount)
        {
            return await _dbSet
                .Where(t => t.StoreId == storeId && t.IsActive &&
                            t.MinimumPrincipal <= principalAmount &&
                            t.MaximumPrincipal >= principalAmount)
                .OrderBy(t => t.DisplayOrder)
                .FirstOrDefaultAsync();
        }
    }

    public interface IStateSpecialRuleRepository : IRepository<StateSpecialRule>
    {
        Task<IEnumerable<StateSpecialRule>> GetStoreStateRulesAsync(int storeId);
        Task<StateSpecialRule?> GetStateRuleAsync(int storeId, string stateCode);
    }

    public class StateSpecialRuleRepository : Repository<StateSpecialRule>, IStateSpecialRuleRepository
    {
        public StateSpecialRuleRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<StateSpecialRule>> GetStoreStateRulesAsync(int storeId)
        {
            return await _dbSet
                .Where(r => r.StoreId == storeId && r.IsActive)
                .OrderBy(r => r.StateName)
                .ToListAsync();
        }

        public async Task<StateSpecialRule?> GetStateRuleAsync(int storeId, string stateCode)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.StoreId == storeId && r.StateCode == stateCode.ToUpper() && r.IsActive);
        }
    }

    public interface IFeeRepository : IRepository<Fee>
    {
        Task<IEnumerable<Fee>> GetCompanyFeesAsync(int companyId);
        Task<Fee?> GetStoreFeeAsync(int storeId, int feeId);
    }

    public class FeeRepository : Repository<Fee>, IFeeRepository
    {
        public FeeRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Fee>> GetCompanyFeesAsync(int companyId)
        {
            return await _dbSet.Where(f => f.CompanyId == companyId).OrderBy(f => f.FeeType).ToListAsync();
        }

        public async Task<Fee?> GetStoreFeeAsync(int storeId, int feeId)
        {
            return await _dbSet.FirstOrDefaultAsync(f => f.Id == feeId);
        }
    }
}
