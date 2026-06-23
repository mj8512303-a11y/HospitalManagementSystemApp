using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } // SuperAdmin, Admin, Doctor, Receptionist, Pharmacist, LabTechnician

        public int? BranchId { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Last Login")]
        public DateTime? LastLogin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
