using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Models
{
    public class Designation
    {
        public int Id { get; set; }

        [Required, Display(Name = "Designation Name")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Display(Name = "Basic Salary")]
        public decimal BasicSalary { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
