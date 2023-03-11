using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace app.Domain.Entities;

[Table("users")]
public class User : BaseModel
{
    [Column("id")]
    public int Id { get; set; }
    [Column("email", TypeName="VARCHAR")]
    [StringLength(250)]
    public string Email { get; set; }
    [Column("password", TypeName="VARCHAR")]
    [StringLength(250)]
    public string Password { get; set; }
    [Column("email_verify_at")]
    public DateTime? EmailVerifyDate { get; set; }
}