using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.DTOs;
using SmartGarage.Models;
using SmartGarage.Interface;
namespace SmartGarage.Services
{
    public class CheckInService : ICheckInService
    {
        private readonly GarageDbContext _context;

        public CheckInService(GarageDbContext context)
        {
            _context = context;
        }

        public async Task<object> ProcessScanAsync(string licensePlate)
        {
            var vehicle = await _context.Vehicles
                                        .Include(v => v.Customer)
                                        .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);

            if (vehicle != null)
            {
                // Khách cũ
                return new
                {
                    status = "existing_customer",
                    message = $"Chào anh/chị {vehicle.Customer.FullName}, xe {vehicle.Make} {vehicle.Model} đã trở lại.",
                    customer = new { fullName = vehicle.Customer.FullName, phoneNumber = vehicle.Customer.PhoneNumber },
                    vehicle = new { licensePlate = vehicle.LicensePlate, make = vehicle.Make, model = vehicle.Model }
                };
            }

            // Khách mới
            return new
            {
                status = "new_customer",
                message = "Xe chưa có trong hệ thống, vui lòng tạo hồ sơ mới.",
                licensePlate = licensePlate
            };
        }

        public async Task<bool> RegisterCustomerAsync(RegisterRequest request)
        {
            try
            {
                var newCustomer = new Customer
                {
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address
                };

                var newVehicle = new Vehicle
                {
                    LicensePlate = request.LicensePlate,
                    Make = request.Make,
                    Model = request.Model,
                    Customer = newCustomer
                };

                _context.Customers.Add(newCustomer);
                _context.Vehicles.Add(newVehicle);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                // Bắt lỗi nếu lưu Database thất bại
                return false;
            }
        }
    }
}