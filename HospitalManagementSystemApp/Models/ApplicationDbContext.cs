using System.Data.Entity;

namespace HospitalMS.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("HospitalMSConnection")
        {
        }

        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Salary> Salaries { get; set; }
        public DbSet<Billing> Billings { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<LabTest> LabTests { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== CASCADE DELETE BAND KARNA =====
            // Yeh fix karta hai: "may cause cycles or multiple cascade paths" error

            // Doctor -> Branch (NO CASCADE)
            modelBuilder.Entity<Doctor>()
                .HasRequired(d => d.Branch)
                .WithMany(b => b.Doctors)
                .HasForeignKey(d => d.BranchId)
                .WillCascadeOnDelete(false);

            // Doctor -> Department (NO CASCADE)
            modelBuilder.Entity<Doctor>()
                .HasRequired(d => d.Department)
                .WithMany(dep => dep.Doctors)
                .HasForeignKey(d => d.DepartmentId)
                .WillCascadeOnDelete(false);

            // Staff -> Branch (NO CASCADE)
            modelBuilder.Entity<Staff>()
                .HasRequired(s => s.Branch)
                .WithMany(b => b.Staffs)
                .HasForeignKey(s => s.BranchId)
                .WillCascadeOnDelete(false);

            // Staff -> Department (NO CASCADE)
            modelBuilder.Entity<Staff>()
                .HasRequired(s => s.Department)
                .WithMany(d => d.Staffs)
                .HasForeignKey(s => s.DepartmentId)
                .WillCascadeOnDelete(false);

            // Staff -> Designation (NO CASCADE)
            modelBuilder.Entity<Staff>()
                .HasRequired(s => s.Designation)
                .WithMany()
                .HasForeignKey(s => s.DesignationId)
                .WillCascadeOnDelete(false);

            // Staff -> Shift (NO CASCADE)
            modelBuilder.Entity<Staff>()
                .HasRequired(s => s.Shift)
                .WithMany()
                .HasForeignKey(s => s.ShiftId)
                .WillCascadeOnDelete(false);

            // Patient -> Branch (NO CASCADE)
            modelBuilder.Entity<Patient>()
                .HasRequired(p => p.Branch)
                .WithMany(b => b.Patients)
                .HasForeignKey(p => p.BranchId)
                .WillCascadeOnDelete(false);

            // Appointment -> Patient (NO CASCADE)
            modelBuilder.Entity<Appointment>()
                .HasRequired(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .WillCascadeOnDelete(false);

            // Appointment -> Doctor (NO CASCADE)
            modelBuilder.Entity<Appointment>()
                .HasRequired(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .WillCascadeOnDelete(false);

            // Appointment -> Branch (NO CASCADE)
            modelBuilder.Entity<Appointment>()
                .HasRequired(a => a.Branch)
                .WithMany()
                .HasForeignKey(a => a.BranchId)
                .WillCascadeOnDelete(false);

            // Billing -> Patient (NO CASCADE)
            modelBuilder.Entity<Billing>()
                .HasRequired(b => b.Patient)
                .WithMany(p => p.Billings)
                .HasForeignKey(b => b.PatientId)
                .WillCascadeOnDelete(false);

            // Billing -> Branch (NO CASCADE)
            modelBuilder.Entity<Billing>()
                .HasRequired(b => b.Branch)
                .WithMany()
                .HasForeignKey(b => b.BranchId)
                .WillCascadeOnDelete(false);

            // Medicine -> Branch (NO CASCADE)
            modelBuilder.Entity<Medicine>()
                .HasRequired(m => m.Branch)
                .WithMany()
                .HasForeignKey(m => m.BranchId)
                .WillCascadeOnDelete(false);

            // LabTest -> Branch (NO CASCADE)
            modelBuilder.Entity<LabTest>()
                .HasRequired(l => l.Branch)
                .WithMany()
                .HasForeignKey(l => l.BranchId)
                .WillCascadeOnDelete(false);

            // Department -> Branch (NORMAL CASCADE - parent delete ho to dept bhi)
            modelBuilder.Entity<Department>()
                .HasRequired(d => d.Branch)
                .WithMany(b => b.Departments)
                .HasForeignKey(d => d.BranchId)
                .WillCascadeOnDelete(true);
        }
    }
}
