using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalMS.Models
{
    public class Billing
    {
        public int Id { get; set; }

        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [ForeignKey("Branch")]
        public int BranchId { get; set; }

        [Display(Name = "Invoice No")]
        public string InvoiceNo { get; set; }

        [Display(Name = "Billing Date")]
        [DataType(DataType.Date)]
        public DateTime BillingDate { get; set; } = DateTime.Now;

        [Display(Name = "Consultation Fee")]
        public decimal ConsultationFee { get; set; }

        [Display(Name = "Test Charges")]
        public decimal TestCharges { get; set; }

        [Display(Name = "Medicine Charges")]
        public decimal MedicineCharges { get; set; }

        [Display(Name = "Other Charges")]
        public decimal OtherCharges { get; set; }

        [Display(Name = "Discount")]
        public decimal Discount { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Paid Amount")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "Remaining Amount")]
        public decimal RemainingAmount { get; set; }

        [Display(Name = "Payment Mode")]
        public string PaymentMode { get; set; } // Cash, Card, Online

        public string Status { get; set; } = "Pending"; // Pending, Partial, Paid

        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Patient Patient { get; set; }
        public virtual Branch Branch { get; set; }
    }
}
