using System.Security.Claims;

namespace app.Domain.Entities;

public interface IAuthenticationUser
{
    public IEnumerable<Claim> Claims();
}