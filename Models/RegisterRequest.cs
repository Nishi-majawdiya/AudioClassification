using System.ComponentModel.DataAnnotations;

public class RegisterRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(20)]
    public string Username { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(6)]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&]).+$",
        ErrorMessage = "Password must contain letter, number, and special character")]
    public string Password { get; set; } = "";
}