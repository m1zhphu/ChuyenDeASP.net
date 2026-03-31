using Microsoft.EntityFrameworkCore;
using SmartGarage.Data;
using SmartGarage.DTOs;
using SmartGarage.Interface;
using SmartGarage.Models;

namespace SmartGarage.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly GarageDbContext _context;

        public CustomerService(GarageDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerResponseDTO>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .Select(c => new CustomerResponseDTO
                {
                    Id = c.Id,
                    FullName = c.FullName ?? string.Empty,
                    PhoneNumber = c.PhoneNumber ?? string.Empty,
                    Address = c.Address ?? string.Empty,
                    CreatedAt = c.CreatedAt
                }).ToListAsync();
        }

        public async Task<CustomerResponseDTO?> GetCustomerByIdAsync(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return null;

            return new CustomerResponseDTO
            {
                Id = customer.Id,
                FullName = customer.FullName ?? string.Empty,
                PhoneNumber = customer.PhoneNumber ?? string.Empty,
                Address = customer.Address ?? string.Empty,
                CreatedAt = customer.CreatedAt
            };
        }

        public async Task<CustomerResponseDTO> CreateCustomerAsync(CustomerRequestDTO request)
        {
            var newCustomer = new Customer
            {
                FullName = request.FullName ?? string.Empty,
                PhoneNumber = request.PhoneNumber ?? string.Empty,
                Address = request.Address ?? string.Empty,
                CreatedAt = DateTime.Now
            };

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();

            return new CustomerResponseDTO
            {
                Id = newCustomer.Id,
                FullName = newCustomer.FullName ?? string.Empty,
                PhoneNumber = newCustomer.PhoneNumber ?? string.Empty,
                Address = newCustomer.Address ?? string.Empty,
                CreatedAt = newCustomer.CreatedAt
            };
        }

        public async Task<bool> UpdateCustomerAsync(Guid id, CustomerRequestDTO request)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;

            customer.FullName = request.FullName ?? string.Empty;
            customer.PhoneNumber = request.PhoneNumber ?? string.Empty;
            customer.Address = request.Address ?? string.Empty;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCustomerAsync(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}