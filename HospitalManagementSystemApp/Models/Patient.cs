using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalMS.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [ForeignKey("Branch")]
        public int BranchId { get; set; }

        [Required, Display(Name = "Patient Name")]
        public string FullName { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; }

        [Display(Name = "Blood Group")]
        public string BloodGroup { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Registration Date")]
        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Display(Name = "Medical History")]
        public string MedicalHistory { get; set; }

        [Display(Name = "Emergency Contact")]
        public string EmergencyContact { get; set; }

        [Display(Name = "Patient Type")]
        public string PatientType { get; set; } = "OPD"; // OPD, IPD

        [Display(Name = "Admission Date")]
        [DataType(DataType.Date)]
        public DateTime? AdmissionDate { get; set; }

        [Display(Name = "Discharge Date")]
        [DataType(DataType.Date)]
        public DateTime? DischargeDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Branch Branch { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<Billing> Billings { get; set; }
    }
}
