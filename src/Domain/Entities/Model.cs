using System.ComponentModel.DataAnnotations.Schema;

namespace app.Domain.Entities;

public class BaseModel
{
    [Column("created_at")]
    public DateTime? CreatedDate { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedDate { get; set; }
}