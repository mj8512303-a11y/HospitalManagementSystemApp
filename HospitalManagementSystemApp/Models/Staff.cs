using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalMS.Models
{
    public class Staff
    {
        public int Id { get; set; }

        [ForeignKey("Branch")]
        public int BranchId { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }

        [ForeignKey("Designation")]
        public int DesignationId { get; set; }

        [ForeignKey("Shift")]
        public int ShiftId { get; set; }

        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        public string CNIC { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string Gender { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Join Date")]
        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; }

        public string Status { get; set; } = "Active"; // Active, OnLeave, Terminated

        [Display(Name = "Document Path")]
        public string DocumentPath { get; set; }

        [Display(Name = "Profile Photo")]
        public string PhotoPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Branch Branch { get; set; }
        public virtual Department Department { get; set; }
        public virtual Designation Designation { get; set; }
        public virtual Shift Shift { get; set; }
    }
}
