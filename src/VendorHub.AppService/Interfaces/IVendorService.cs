using VendorHub.Contracts.Vendor;

namespace VendorHub.AppService.Interfaces;

public interface IVendorService
{
    Task<VendorResponseDto> ApplyAsync(int userId, VendorApplyDto request);
    Task<VendorResponseDto> ApproveAsync(int vendorId);
    Task<VendorResponseDto> RejectAsync(int vendorId);
    Task<VendorResponseDto> GetMyStatusAsync(int userId);
    Task<List<VendorResponseDto>> GetAllAsync();
}
