using AutoMapper;
using Microsoft.Extensions.Logging;
using VehicleInsurance.Application.Customers.Dtos;
using VehicleInsurance.Domain.Customers;
using VehicleInsurance.Application.Customers.Interfaces;
using VehicleInsurance.Domain.Common.Exceptions;
using System.Threading;

namespace VehicleInsurance.Application.Customers.Services;

public class CustomerService
{
    private readonly ICustomerRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ICustomerRepository repo, IMapper mapper, ILogger<CustomerService> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CustomerDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        _logger.LogInformation("🔍 [GetByIdAsync] Fetching customer with ID = {Id}", id);
        var entity = await _repo.GetByIdAsync(id, ct);


        if (entity is null)
        {
            _logger.LogWarning("⚠️ [GetByIdAsync] Customer with ID = {Id} not found", id);

            throw new ForbiddenAppException("customer not found");
        }

        _logger.LogInformation("✅ [GetByIdAsync] Found customer with ID = {Id}", id);
        return _mapper.Map<CustomerDto>(entity);
    }

    public async Task<CustomerDto> CreateAsync(CustomerCreateRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("🆕 [CreateAsync] Creating new customer for UserId = {UserId}", request.UserId);

        var existing = await _repo.GetByUserIdAsync(request.UserId, ct);
        if (existing != null)
        {
            _logger.LogWarning("⚠️ [CreateAsync] UserId = {UserId} already has a customer record", request.UserId);
            throw new ForbiddenAppException("User already assigned to a customer");
        }

        var entity = _mapper.Map<Customer>(request);//copy tất cả các thuộc tính có cùng tên từ CustomerCreateRequest sang Customer

        try
        {
            await _repo.AddAsync(entity, ct);
            _logger.LogInformation("✅ [CreateAsync] Customer created successfully with generated ID = {CustomerId}", entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 [CreateAsync] Failed to create customer for UserId = {UserId}", request.UserId);
            throw new BadRequestAppException("error sever");
        }

        return _mapper.Map<CustomerDto>(entity);
    }

    public async Task<CustomerDto?> UpdateAsync(long id, CustomerUpdateRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("✏️ [UpdateAsync] Updating customer ID = {Id}", id);

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null)
        {
            _logger.LogWarning("⚠️ [UpdateAsync] Customer ID = {Id} not found", id);
            throw new NotFoundException("Customer not found");
        }

        _mapper.Map(request, entity);

        try
        {
            await _repo.UpdateAsync(entity, ct);
            _logger.LogInformation("✅ [UpdateAsync] Customer ID = {Id} updated successfully", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 [UpdateAsync] Failed to update customer ID = {Id}", id);
            throw;
        }

        return _mapper.Map<CustomerDto>(entity);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        _logger.LogInformation("🗑️ [DeleteAsync] Deleting customer ID = {Id}", id);

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null)
        {
            _logger.LogWarning("⚠️ [DeleteAsync] Customer ID = {Id} not found", id);
            throw new NotFoundException("Customer not found");
        }

        try
        {
            await _repo.DeleteAsync(entity, ct);
            _logger.LogInformation("✅ [DeleteAsync] Customer ID = {Id} deleted successfully", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 [DeleteAsync] Failed to delete customer ID = {Id}", id);
            throw;
        }
    }
}
