using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalMS.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required, ForeignKey("Branch")]
        public int BranchId { get; set; }

        [Required, Display(Name = "Department Name")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Display(Name = "Department Head")]
        public string DepartmentHead { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Branch Branch { get; set; }
        public virtual ICollection<Staff> Staffs { get; set; }
        public virtual ICollection<Doctor> Doctors { get; set; }
    }
}
