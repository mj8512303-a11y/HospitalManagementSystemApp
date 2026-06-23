using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Models
{
    public class Hospital
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hospital name is required")]
        [Display(Name = "Hospital Name")]
        public string Name { get; set; }

        [Display(Name = "Registration No")]
        public string RegistrationNo { get; set; }

        // ? NOT Required — optional field
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Contact No")]
        public string ContactNo { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        public string Website { get; set; }

        public string LogoPath { get; set; }

        [Display(Name = "Working Days")]
        public string WorkingDays { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Branch> Branches { get; set; }
    }
}
