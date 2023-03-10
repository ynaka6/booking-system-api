using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Security.Claims;

namespace app.Domain.Entities;

[Table("users")]
public class User : BaseModel, IAuthenticationUser
{
    [Column("id")]
    public int Id { get; set; }
    [Column("email", TypeName = "VARCHAR")]
    [StringLength(250)]
    public string Email { get; set; }
    [Column("password", TypeName = "VARCHAR")]
    [StringLength(250)]
    public string Password { get; set; }
    [Column("email_verify_at")]
    public DateTime? EmailVerifyDate { get; set; }

    [JsonIgnore]
    public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public IEnumerable<Claim> Claims()
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, Email) };
        return claims;
    }
}