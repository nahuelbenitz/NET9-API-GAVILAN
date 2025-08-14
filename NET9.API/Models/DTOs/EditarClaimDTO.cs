using System.ComponentModel.DataAnnotations;

namespace NET9.API.Models.DTOs
{
    public class EditarClaimDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
