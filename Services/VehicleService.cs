using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Services
{
    public interface IVehicleService
    {
        Task<Vehicle> CreateVehicleAsync(Vehicle vehicle);
        Task<Vehicle?> GetVehicleAsync(int id);
        Task<List<Vehicle>> GetCustomerVehiclesAsync(int customerId);
        Task<bool> UpdateVehicleAsync(Vehicle vehicle);
        Task<bool> DeactivateVehicleAsync(int vehicleId);
    }

    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VehicleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Vehicle> CreateVehicleAsync(Vehicle vehicle)
        {
            vehicle.CreatedDate = DateTime.UtcNow;
            vehicle.IsActive = true;
            await _unitOfWork.Vehicles.AddAsync(vehicle);
            await _unitOfWork.SaveChangesAsync();
            return vehicle;
        }

        public async Task<Vehicle?> GetVehicleAsync(int id)
        {
            return await _unitOfWork.Vehicles.GetVehicleWithTitlePawnsAsync(id);
        }

        public async Task<List<Vehicle>> GetCustomerVehiclesAsync(int customerId)
        {
            var vehicles = await _unitOfWork.Vehicles.GetCustomerVehiclesAsync(customerId);
            return vehicles.ToList();
        }

        public async Task<bool> UpdateVehicleAsync(Vehicle vehicle)
        {
            await _unitOfWork.Vehicles.UpdateAsync(vehicle);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateVehicleAsync(int vehicleId)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
            if (vehicle == null)
                return false;

            vehicle.IsActive = false;
            await _unitOfWork.Vehicles.UpdateAsync(vehicle);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
