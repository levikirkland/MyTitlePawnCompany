using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Services
{
    public interface ICustomerService
    {
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer?> GetCustomerAsync(int id);
        Task<List<Customer>> GetCompanyCustomersAsync(int companyId);
        Task<bool> UpdateCustomerAsync(Customer customer);
        Task<bool> DeactivateCustomerAsync(int customerId);
    }

    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            customer.CreatedDate = DateTime.UtcNow;
            customer.IsActive = true;
            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer?> GetCustomerAsync(int id)
        {
            return await _unitOfWork.Customers.GetCustomerWithVehiclesAsync(id);
        }

        public async Task<List<Customer>> GetCompanyCustomersAsync(int companyId)
        {
            var customers = await _unitOfWork.Customers.GetCompanyCustomersAsync(companyId);
            return customers.ToList();
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            await _unitOfWork.Customers.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateCustomerAsync(int customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                return false;

            customer.IsActive = false;
            await _unitOfWork.Customers.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
