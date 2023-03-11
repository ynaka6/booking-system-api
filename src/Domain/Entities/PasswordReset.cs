using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace app.Domain.Entities;

[Table("password_resets")]
public class PasswordReset : BaseModel
{
    [Column("id")]
    public int Id { get; set; }
    [Column("email", TypeName="VARCHAR")]
    [StringLength(250)]
    public string Email { get; set; }
    [Column("token", TypeName="VARCHAR")]
    [StringLength(250)]
    public string Token { get; set; }
}