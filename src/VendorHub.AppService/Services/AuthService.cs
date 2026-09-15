using VendorHub.AppService.Interfaces;
using VendorHub.Contracts.Auth;
using VendorHub.Domain.Entities;
using VendorHub.Domain.Enums;

namespace VendorHub.AppService.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("A user with this email already exists.");

        // Caught here rather than as a foreign-key violation on save, which would
        // surface as an unhandled 500 instead of a readable message.
        if (!Enum.IsDefined((RoleType)request.RoleId))
        {
            var allowed = Enum.GetValues<RoleType>().Select(r => $"{(int)r} ({r})");
            throw new InvalidOperationException($"'{request.RoleId}' is not a valid role id. Allowed values: {string.Join(", ", allowed)}.");
        }

        var (hash, salt) = _passwordHasher.HashPassword(request.Password);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            RoleId = request.RoleId
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Re-read so the Role navigation is loaded — the token includes the role name,
        // and the freshly constructed entity above only carries RoleId.
        var createdUser = await _userRepository.GetByIdAsync(user.Id)
            ?? throw new InvalidOperationException("The user was created but could not be read back.");

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(createdUser);

        return new AuthResponseDto
        {
            UserId = createdUser.Id,
            Name = createdUser.Name,
            Email = createdUser.Email,
            Role = ((RoleType)createdUser.RoleId).ToString(),
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var isValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);
        if (!isValid)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = ((RoleType)user.RoleId).ToString(),
            Token = token,
            ExpiresAt = expiresAt
        };
    }
}