using VehicleInsurance.Domain.Customers;
using System.Threading;
using System.Threading.Tasks;

namespace VehicleInsurance.Application.Customers.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Customer?> GetByUserIdAsync(long userId, CancellationToken ct = default); // <-- long
    Task AddAsync(Customer entity, CancellationToken ct = default);
    Task UpdateAsync(Customer entity, CancellationToken ct = default);
    Task DeleteAsync(Customer entity, CancellationToken ct = default);
}
