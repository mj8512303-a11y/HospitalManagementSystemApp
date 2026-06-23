using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Models
{
    public class Shift
    {
        public int Id { get; set; }

        [Required, Display(Name = "Shift Name")]
        public string Name { get; set; }

        [Required, Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }

        [Required, Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; }

        public string Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
