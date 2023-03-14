using app.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Security.Claims;

namespace app.Domain.Entities;

[Table("admin_users")]
public class AdminUser : BaseModel, IAuthenticationUser
{
    [Column("id")]
    public int Id { get; set; }
    [Column("email", TypeName = "VARCHAR")]
    [StringLength(250)]
    public string Email { get; set; }
    [Column("password", TypeName = "VARCHAR")]
    [StringLength(250)]
    public string Password { get; set; }
    [Required]
    [Column("admin_user_role", TypeName = "ENUM('Administrator', 'Operator', 'ReadOnly')")]
    [StringLength(250)]
    public AdminUserRole AdminUserRole { get; set; }

    [JsonIgnore]
    public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public IEnumerable<Claim> Claims()
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, Email) };
        return claims;
    }
}