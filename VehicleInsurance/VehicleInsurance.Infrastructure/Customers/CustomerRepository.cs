using VehicleInsurance.Domain.Customers;
using VehicleInsurance.Application.Customers.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using VehicleInsurance.Infrastructure.Data;
namespace VehicleInsurance.Infrastructure.Customers;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        _context.Customers.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        _context.Customers.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
