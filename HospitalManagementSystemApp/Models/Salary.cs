using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Models
{
    public class Salary
    {
        public int Id { get; set; }

        [Required, Display(Name = "Employee ID")]
        public int EmployeeId { get; set; }

        [Required, Display(Name = "Employee Type")]
        public string EmployeeType { get; set; } // Staff or Doctor

        [Required, Display(Name = "Employee Name")]
        public string EmployeeName { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        [Display(Name = "Basic Salary")]
        public decimal BasicSalary { get; set; }

        public decimal Allowances { get; set; }

        public decimal Bonus { get; set; }

        public decimal Deductions { get; set; }

        [Display(Name = "Overtime Amount")]
        public decimal OvertimeAmount { get; set; }

        [Display(Name = "Net Salary")]
        public decimal NetSalary { get; set; }

        [Display(Name = "Present Days")]
        public int PresentDays { get; set; }

        [Display(Name = "Absent Days")]
        public int AbsentDays { get; set; }

        [Display(Name = "Is Paid")]
        public bool IsPaid { get; set; } = false;

        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }

        public string Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
