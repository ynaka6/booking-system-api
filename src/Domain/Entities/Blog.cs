using System.ComponentModel.DataAnnotations.Schema;

namespace app.Domain.Entities;

[Table("blogs")]
public class Blog : BaseModel
{
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    public string Name { get; set; }
}