using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace app.Domain.Entities;

[Table("refresh_tokens")]
public class RefreshToken : BaseModel
{
    [Column("id")]
    public int Id { get; set; }
    [Column("user_id")]
    [ForeignKey("User")]
    public int UserId { get; set; }
    [Column("token", TypeName = "VARCHAR")]
    [StringLength(250)]
    public string Token { get; set; }
    [Column("expired_at")]
    public DateTime? ExpiredDate { get; set; }
    [Column("created_by_ip")]
    public string CreatedByIp { get; set; }

    public User User { get; set; }


    public bool IsExpired => DateTime.Now >= ExpiredDate;
    public bool IsActive => !IsExpired;
}