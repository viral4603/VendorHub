using VendorHub.AppService.Interfaces;
using VendorHub.Contracts.Vendor;
using VendorHub.Domain.Enums;
using VendorEntity = VendorHub.Domain.Entities.Vendor;

namespace VendorHub.AppService.Services;

public class VendorService : IVendorService
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IUserRepository _userRepository;

    public VendorService(IVendorRepository vendorRepository, IUserRepository userRepository)
    {
        _vendorRepository = vendorRepository;
        _userRepository = userRepository;
    }

    public async Task<VendorResponseDto> ApplyAsync(int userId, VendorApplyDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var existing = await _vendorRepository.GetByUserIdAsync(userId);
        if (existing != null)
            throw new InvalidOperationException("A vendor application already exists for this user.");

        if (string.IsNullOrWhiteSpace(request.ShopName))
            throw new InvalidOperationException("Shop name is required.");

        var vendor = new VendorEntity
        {
            UserId = userId,
            ShopName = request.ShopName.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Status = VendorStatus.Pending
        };

        await _vendorRepository.AddAsync(vendor);
        await _vendorRepository.SaveChangesAsync();

        vendor.User = user;
        return MapToDto(vendor);
    }

    public async Task<VendorResponseDto> ApproveAsync(int vendorId) =>
        await ReviewAsync(vendorId, VendorStatus.Approved);

    public async Task<VendorResponseDto> RejectAsync(int vendorId) =>
        await ReviewAsync(vendorId, VendorStatus.Rejected);

    public async Task<VendorResponseDto> GetMyStatusAsync(int userId)
    {
        var vendor = await _vendorRepository.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("No vendor application found for this user.");

        return MapToDto(vendor);
    }

    public async Task<List<VendorResponseDto>> GetAllAsync()
    {
        var vendors = await _vendorRepository.GetAllAsync();
        return vendors.Select(MapToDto).ToList();
    }

    private async Task<VendorResponseDto> ReviewAsync(int vendorId, VendorStatus status)
    {
        var vendor = await _vendorRepository.GetByIdAsync(vendorId)
            ?? throw new KeyNotFoundException("Vendor not found.");

        if (vendor.Status != VendorStatus.Pending)
            throw new InvalidOperationException($"This vendor application has already been {vendor.Status.ToString().ToLower()}.");

        vendor.Status = status;
        vendor.ReviewedAt = DateTime.UtcNow;

        await _vendorRepository.SaveChangesAsync();

        return MapToDto(vendor);
    }

    private static VendorResponseDto MapToDto(VendorEntity vendor) => new()
    {
        Id = vendor.Id,
        UserId = vendor.UserId,
        OwnerName = vendor.User?.Name ?? string.Empty,
        ShopName = vendor.ShopName,
        Description = vendor.Description,
        Status = vendor.Status.ToString(),
        CreatedAt = vendor.CreatedAt,
        ReviewedAt = vendor.ReviewedAt
    };
}
