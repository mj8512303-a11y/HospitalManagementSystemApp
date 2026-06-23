using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        [Required, Display(Name = "Employee ID")]
        public int EmployeeId { get; set; }

        [Required, Display(Name = "Employee Type")]
        public string EmployeeType { get; set; } // Staff or Doctor

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Display(Name = "In Time")]
        public TimeSpan? InTime { get; set; }

        [Display(Name = "Out Time")]
        public TimeSpan? OutTime { get; set; }

        [Required]
        public string Status { get; set; } = "Present"; // Present, Absent, Late, HalfDay, Leave

        public string Remarks { get; set; }

        [Display(Name = "Marked By")]
        public string MarkedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
