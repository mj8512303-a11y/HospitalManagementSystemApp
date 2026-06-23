using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalMS.Models
{
    public class Medicine
    {
        public int Id { get; set; }

        [ForeignKey("Branch")]
        public int BranchId { get; set; }

        [Required, Display(Name = "Medicine Name")]
        public string Name { get; set; }

        public string Category { get; set; }

        public string Manufacturer { get; set; }

        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }

        [Display(Name = "Minimum Stock Alert")]
        public int MinStockAlert { get; set; } = 10;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Branch Branch { get; set; }
    }

    public class LabTest
    {
        public int Id { get; set; }

        public int PatientId { get; set; }

        public int DoctorId { get; set; }

        [ForeignKey("Branch")]
        public int BranchId { get; set; }

        [Required, Display(Name = "Test Name")]
        public string TestName { get; set; }

        [Display(Name = "Test Date")]
        [DataType(DataType.Date)]
        public DateTime TestDate { get; set; } = DateTime.Now;

        public decimal Charges { get; set; }

        public string Result { get; set; }

        [Display(Name = "Report Path")]
        public string ReportPath { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Completed

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Branch Branch { get; set; }
    }
}
