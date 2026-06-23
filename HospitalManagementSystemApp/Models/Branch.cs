using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalMS.Models
{
    public class Branch
    {
        public int Id { get; set; }

        [Required, ForeignKey("Hospital")]
        public int HospitalId { get; set; }

        [Required, Display(Name = "Branch Name")]
        public string Name { get; set; }

        [Display(Name = "Branch Code")]
        public string Code { get; set; }

        public string Location { get; set; }

        [Display(Name = "Contact No")]
        public string ContactNo { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Working Hours")]
        public string WorkingHours { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Hospital Hospital { get; set; }
        public virtual ICollection<Department> Departments { get; set; }
        public virtual ICollection<Staff> Staffs { get; set; }
        public virtual ICollection<Doctor> Doctors { get; set; }
        public virtual ICollection<Patient> Patients { get; set; }
    }
}
