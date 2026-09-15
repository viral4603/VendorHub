using VendorHub.Contracts.Vendor;

namespace VendorHub.AppService.Interfaces;

public interface IVendorService
{
    Task<VendorResponseDto> ApplyAsync(int userId, VendorApplyDto request);
    Task<VendorResponseDto> ReviewAsync(int vendorId, ReviewVendorRequestDto request);
    Task<VendorResponseDto> GetMyStatusAsync(int userId);
    Task<List<VendorResponseDto>> GetAllAsync();
}
