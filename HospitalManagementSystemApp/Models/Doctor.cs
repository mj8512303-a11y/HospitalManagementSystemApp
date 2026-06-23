using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalMS.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [ForeignKey("Branch")]
        public int BranchId { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }

        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        public string Specialization { get; set; }

        public string Qualification { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Phone { get; set; }

        [Display(Name = "Available Days")]
        public string AvailableDays { get; set; }

        [Display(Name = "Consultation Fee")]
        public decimal ConsultationFee { get; set; }

        [Display(Name = "Profile Photo")]
        public string PhotoPath { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Branch Branch { get; set; }
        public virtual Department Department { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
    }
}
