namespace app.Models;

public class User : BaseModel
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}