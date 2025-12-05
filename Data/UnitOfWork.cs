using MyTitlePawnCompany.Data.Repositories;

namespace MyTitlePawnCompany.Data
{
    public interface IUnitOfWork : IDisposable
    {
        ICompanyRepository Companies { get; }
        ICustomerRepository Customers { get; }
        ICustomerReferenceRepository CustomerReferences { get; }
        IVehicleRepository Vehicles { get; }
        ITitlePawnRepository TitlePawns { get; }
        IPaymentRepository Payments { get; }
        IVendorRepository Vendors { get; }
        IReportRepository Reports { get; }
        IStoreRepository Stores { get; }
        IStoreUserRepository StoreUsers { get; }
        IInterestRateTierRepository InterestRateTiers { get; }
        IStateSpecialRuleRepository StateSpecialRules { get; }
        IFeeRepository Fees { get; }
        Task SaveChangesAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private ICompanyRepository? _companyRepository;
        private ICustomerRepository? _customerRepository;
        private ICustomerReferenceRepository? _customerReferenceRepository;
        private IVehicleRepository? _vehicleRepository;
        private ITitlePawnRepository? _titlePawnRepository;
        private IPaymentRepository? _paymentRepository;
        private IVendorRepository? _vendorRepository;
        private IReportRepository? _reportRepository;
        private IStoreRepository? _storeRepository;
        private IStoreUserRepository? _storeUserRepository;
        private IInterestRateTierRepository? _interestRateTierRepository;
        private IStateSpecialRuleRepository? _stateSpecialRuleRepository;
        private IFeeRepository? _feeRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public ICompanyRepository Companies => _companyRepository ??= new CompanyRepository(_context);
        public ICustomerRepository Customers => _customerRepository ??= new CustomerRepository(_context);
        public ICustomerReferenceRepository CustomerReferences => _customerReferenceRepository ??= new CustomerReferenceRepository(_context);
        public IVehicleRepository Vehicles => _vehicleRepository ??= new VehicleRepository(_context);
        public ITitlePawnRepository TitlePawns => _titlePawnRepository ??= new TitlePawnRepository(_context);
        public IPaymentRepository Payments => _paymentRepository ??= new PaymentRepository(_context);
        public IVendorRepository Vendors => _vendorRepository ??= new VendorRepository(_context);
        public IReportRepository Reports => _reportRepository ??= new ReportRepository(_context);
        public IStoreRepository Stores => _storeRepository ??= new StoreRepository(_context);
        public IStoreUserRepository StoreUsers => _storeUserRepository ??= new StoreUserRepository(_context);
        public IInterestRateTierRepository InterestRateTiers => _interestRateTierRepository ??= new InterestRateTierRepository(_context);
        public IStateSpecialRuleRepository StateSpecialRules => _stateSpecialRuleRepository ??= new StateSpecialRuleRepository(_context);
        public IFeeRepository Fees => _feeRepository ??= new FeeRepository(_context);

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
