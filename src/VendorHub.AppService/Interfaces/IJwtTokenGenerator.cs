using VendorHub.Domain.Entities;

namespace VendorHub.AppService.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}