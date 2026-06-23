namespace HospitalManagementSystemApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Appointments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PatientId = c.Int(nullable: false),
                        DoctorId = c.Int(nullable: false),
                        BranchId = c.Int(nullable: false),
                        AppointmentDate = c.DateTime(nullable: false),
                        TimeSlot = c.String(),
                        Status = c.String(),
                        Notes = c.String(),
                        BookedBy = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Branches", t => t.BranchId)
                .ForeignKey("dbo.Doctors", t => t.DoctorId)
                .ForeignKey("dbo.Patients", t => t.PatientId)
                .Index(t => t.PatientId)
                .Index(t => t.DoctorId)
                .Index(t => t.BranchId);
            
            CreateTable(
                "dbo.Branches",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HospitalId = c.Int(nullable: false),
                        Name = c.String(nullable: false),
                        Code = c.String(),
                        Location = c.String(),
                        ContactNo = c.String(),
                        Email = c.String(),
                        WorkingHours = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Hospitals", t => t.HospitalId, cascadeDelete: true)
                .Index(t => t.HospitalId);
            
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BranchId = c.Int(nullable: false),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        DepartmentHead = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Branches", t => t.BranchId, cascadeDelete: true)
                .Index(t => t.BranchId);
            
            CreateTable(
                "dbo.Doctors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BranchId = c.Int(nullable: false),
                        DepartmentId = c.Int(nullable: false),
                        FullName = c.String(nullable: false),
                        Specialization = c.String(nullable: false),
                        Qualification = c.String(),
                        Email = c.String(),
                        Phone = c.String(),
                        AvailableDays = c.String(),
                        ConsultationFee = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PhotoPath = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Branches", t => t.BranchId)
                .ForeignKey("dbo.Departments", t => t.DepartmentId)
                .Index(t => t.BranchId)
                .Index(t => t.DepartmentId);
            
            CreateTable(
                "dbo.Staffs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BranchId = c.Int(nullable: false),
                        DepartmentId = c.Int(nullable: false),
                        DesignationId = c.Int(nullable: false),
                        ShiftId = c.Int(nullable: false),
                        FullName = c.String(nullable: false),
                        CNIC = c.String(nullable: false),
                        Email = c.String(),
                        Phone = c.String(),
                        Address = c.String(),
                        Gender = c.String(),
                        DateOfBirth = c.DateTime(),
                        JoinDate = c.DateTime(nullable: false),
                        Status = c.String(),
                        DocumentPath = c.String(),
                        PhotoPath = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Branches", t => t.BranchId)
                .ForeignKey("dbo.Departments", t => t.DepartmentId)
                .ForeignKey("dbo.Designations", t => t.DesignationId)
                .ForeignKey("dbo.Shifts", t => t.ShiftId)
                .Index(t => t.BranchId)
                .Index(t => t.DepartmentId)
                .Index(t => t.DesignationId)
                .Index(t => t.ShiftId);
            
            CreateTable(
                "dbo.Designations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        BasicSalary = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Shifts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Hospitals",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        RegistrationNo = c.String(),
                        Address = c.String(nullable: false),
                        ContactNo = c.String(),
                        Email = c.String(),
                        Website = c.String(),
                        LogoPath = c.String(),
                        WorkingDays = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Patients",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BranchId = c.Int(nullable: false),
                        FullName = c.String(nullable: false),
                        Age = c.Int(nullable: false),
                        Gender = c.String(nullable: false),
                        BloodGroup = c.String(),
                        Address = c.String(),
                        Phone = c.String(),
                        Email = c.String(),
                        RegistrationDate = c.DateTime(nullable: false),
                        MedicalHistory = c.String(),
                        EmergencyContact = c.String(),
                        PatientType = c.String(),
                        AdmissionDate = c.DateTime(),
                        DischargeDate = c.DateTime(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Branches", t => t.BranchId)
                .Index(t => t.BranchId);
            
            CreateTable(
                "dbo.Billings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PatientId = c.Int(nullable: false),
                        BranchId = c.Int(nullable: false),
                        InvoiceNo = c.String(),
                        BillingDate = c.DateTime(nullable: false),
                        ConsultationFee = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TestCharges = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MedicineCharges = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OtherCharges = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Discount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaidAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RemainingAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentMode = c.String(),
                        Status = c.String(),
                        Notes = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Branches", t => t.BranchId)
                .ForeignKey("dbo.Patients", t => t.PatientId)
                .Index(t => t.PatientId)
                .Index(t => t.BranchId);
            
            CreateTable(
                "dbo.AppUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false),
                        Username = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        PasswordHash = c.String(nullable: false),
                        Role = c.String(nullable: false),
                        BranchId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        LastLogin = c.DateTime(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Attendances",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EmployeeId = c.Int(nullable: false),
                        EmployeeType = c.String(nullable: false),
                        Date = c.DateTime(nullable: false),
                        InTime = c.Time(precision: 7),
                        OutTime = c.Time(precision: 7),
                        Status = c.String(nullable: false),
                        Remarks = c.String(),
                        MarkedBy = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LabTests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PatientId = c.Int(nullable: false),
                        DoctorId = c.Int(nullable: false),
                        BranchId = c.Int(nullable: false),
                        TestName = c.String(nullable: false),
                        TestDate = c.DateTime(nullable: false),
                        Charges = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Result = c.String(),
                        ReportPath = c.String(),
                        Status = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Branches", t => t.BranchId)
                .Index(t => t.BranchId);
            
            CreateTable(
                "dbo.Medicines",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BranchId = c.Int(nullable: false),
                        Name = c.String(nullable: false),
                        Category = c.String(),
                        Manufacturer = c.String(),
                        StockQuantity = c.Int(nullable: false),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ExpiryDate = c.DateTime(nullable: false),
                        MinStockAlert = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Branches", t => t.BranchId)
                .Index(t => t.BranchId);
            
            CreateTable(
                "dbo.Salaries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EmployeeId = c.Int(nullable: false),
                        EmployeeType = c.String(nullable: false),
                        EmployeeName = c.String(nullable: false),
                        Month = c.Int(nullable: false),
                        Year = c.Int(nullable: false),
                        BasicSalary = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Allowances = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Bonus = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Deductions = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OvertimeAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        NetSalary = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PresentDays = c.Int(nullable: false),
                        AbsentDays = c.Int(nullable: false),
                        IsPaid = c.Boolean(nullable: false),
                        PaymentDate = c.DateTime(),
                        Remarks = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Medicines", "BranchId", "dbo.Branches");
            DropForeignKey("dbo.LabTests", "BranchId", "dbo.Branches");
            DropForeignKey("dbo.Appointments", "PatientId", "dbo.Patients");
            DropForeignKey("dbo.Appointments", "DoctorId", "dbo.Doctors");
            DropForeignKey("dbo.Appointments", "BranchId", "dbo.Branches");
            DropForeignKey("dbo.Patients", "BranchId", "dbo.Branches");
            DropForeignKey("dbo.Billings", "PatientId", "dbo.Patients");
            DropForeignKey("dbo.Billings", "BranchId", "dbo.Branches");
            DropForeignKey("dbo.Branches", "HospitalId", "dbo.Hospitals");
            DropForeignKey("dbo.Staffs", "ShiftId", "dbo.Shifts");
            DropForeignKey("dbo.Staffs", "DesignationId", "dbo.Designations");
            DropForeignKey("dbo.Staffs", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.Staffs", "BranchId", "dbo.Branches");
            DropForeignKey("dbo.Doctors", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.Doctors", "BranchId", "dbo.Branches");
            DropForeignKey("dbo.Departments", "BranchId", "dbo.Branches");
            DropIndex("dbo.Medicines", new[] { "BranchId" });
            DropIndex("dbo.LabTests", new[] { "BranchId" });
            DropIndex("dbo.Billings", new[] { "BranchId" });
            DropIndex("dbo.Billings", new[] { "PatientId" });
            DropIndex("dbo.Patients", new[] { "BranchId" });
            DropIndex("dbo.Staffs", new[] { "ShiftId" });
            DropIndex("dbo.Staffs", new[] { "DesignationId" });
            DropIndex("dbo.Staffs", new[] { "DepartmentId" });
            DropIndex("dbo.Staffs", new[] { "BranchId" });
            DropIndex("dbo.Doctors", new[] { "DepartmentId" });
            DropIndex("dbo.Doctors", new[] { "BranchId" });
            DropIndex("dbo.Departments", new[] { "BranchId" });
            DropIndex("dbo.Branches", new[] { "HospitalId" });
            DropIndex("dbo.Appointments", new[] { "BranchId" });
            DropIndex("dbo.Appointments", new[] { "DoctorId" });
            DropIndex("dbo.Appointments", new[] { "PatientId" });
            DropTable("dbo.Salaries");
            DropTable("dbo.Medicines");
            DropTable("dbo.LabTests");
            DropTable("dbo.Attendances");
            DropTable("dbo.AppUsers");
            DropTable("dbo.Billings");
            DropTable("dbo.Patients");
            DropTable("dbo.Hospitals");
            DropTable("dbo.Shifts");
            DropTable("dbo.Designations");
            DropTable("dbo.Staffs");
            DropTable("dbo.Doctors");
            DropTable("dbo.Departments");
            DropTable("dbo.Branches");
            DropTable("dbo.Appointments");
        }
    }
}
