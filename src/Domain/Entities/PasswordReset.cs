namespace app.Domain.Entities;

public class PasswordReset : BaseModel
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
}