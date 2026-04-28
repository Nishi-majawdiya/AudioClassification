using System.ComponentModel.DataAnnotations;

namespace AudioClassification.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string CreatedBy { get; set; } = "Nishi";

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

       

        public string? UpdatedBy { get; set; }

      
        public DateTime? UpdatedDate { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;


    }
}