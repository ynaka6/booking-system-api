using app.Domain.Entities;

namespace app.Application.Services;

public interface IAuthenticationService
{
    public string GenerateToken(IAuthenticationUser user);
    // public int? ValidateToken(string token);
    public RefreshToken GenerateRefreshToken(string ipAddress);
}