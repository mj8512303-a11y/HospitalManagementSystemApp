using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalMS.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }

        [ForeignKey("Branch")]
        public int BranchId { get; set; }

        [Required, Display(Name = "Appointment Date")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Time Slot")]
        public string TimeSlot { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Approved, Completed, Cancelled

        public string Notes { get; set; }

        [Display(Name = "Booked By")]
        public string BookedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Patient Patient { get; set; }
        public virtual Doctor Doctor { get; set; }
        public virtual Branch Branch { get; set; }
    }
}
