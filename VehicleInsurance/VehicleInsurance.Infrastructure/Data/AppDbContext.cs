using VehicleInsurance.Domain.Entity; // thêm namespace domain entity Vehicle
using VehicleInsurance.Domain.Policies;
using VehicleInsurance.Domain.Billings;

namespace VehicleInsurance.Infrastructure.Data
{

    using Microsoft.EntityFrameworkCore;
    using VehicleInsurance.Domain.Users;
    using VehicleInsurance.Domain.Auth;
    using VehicleInsurance.Domain.EmailVerification;
    using VehicleInsurance.Domain.Roles;
    using VehicleInsurance.Domain.Customers; // <-- thêm

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

        // DbSets
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();   // <-- thêm dòng này
                                                            // DbSets
        public DbSet<Estimate> Estimates => Set<Estimate>();
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Customer> Customers => Set<Customer>();   // <-- thêm
        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<Billing> Billings => Set<Billing>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            // ===== USERS (giữ nguyên)
            b.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Username).HasColumnName("username").IsRequired().HasMaxLength(100);
                e.Property(x => x.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
                e.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired().HasMaxLength(255);
                e.Property(x => x.Role).HasColumnName("role").IsRequired().HasMaxLength(32);
                e.Property(x => x.Active).HasColumnName("active");

                e.HasIndex(x => x.Username).IsUnique();
                e.HasIndex(x => x.Email).IsUnique();
            });

            // ===== REFRESH TOKENS (giữ nguyên)
            b.Entity<RefreshToken>(e =>
            {
                e.ToTable("refresh_tokens");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
                e.Property(x => x.TokenHash).HasColumnName("token_hash").IsRequired().HasMaxLength(64);
                e.Property(x => x.TokenFamily).HasColumnName("token_family").HasMaxLength(36);
                e.Property(x => x.IssuedAt).HasColumnName("issued_at");
                e.Property(x => x.ExpiresAt).HasColumnName("expires_at");
                e.Property(x => x.Revoked).HasColumnName("revoked");
                e.Property(x => x.RevokedAt).HasColumnName("revoked_at");
                e.Property(x => x.ReplacedByTokenHash).HasColumnName("replaced_by_token_hash").HasMaxLength(64);
                e.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
                e.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(255);
                e.Property(x => x.CreatedAt).HasColumnName("created_at");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

                e.HasIndex(x => x.TokenHash).IsUnique();
                e.HasIndex(x => x.ExpiresAt);
                e.HasIndex(x => x.Revoked);

                e.HasOne<User>()
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .HasConstraintName("fk_refresh_tokens_user")
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== EMAIL VERIFICATION TOKENS (giữ nguyên)
            b.Entity<EmailVerificationToken>(e =>
            {
                e.ToTable("email_verification_tokens");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
                e.Property(x => x.Token).HasColumnName("token").IsRequired().HasMaxLength(255);
                e.Property(x => x.ExpiresAtUtc).HasColumnName("expires_at").IsRequired();
                e.Property(x => x.UsedAtUtc).HasColumnName("used_at");
                e.Property(x => x.CreatedAtUtc).HasColumnName("created_at").IsRequired();

                e.HasIndex(x => x.Token).IsUnique();
                e.HasIndex(x => new { x.UserId, x.UsedAtUtc });
            });

            // ===== ROLES (giữ nguyên)
            b.Entity<Role>(e =>
            {
                e.ToTable("roles");
                e.HasKey(r => r.RoleId);

                e.Property(r => r.RoleId).HasColumnName("RoleId").ValueGeneratedOnAdd();
                e.Property(r => r.Name).HasColumnName("Name").IsRequired().HasMaxLength(50);
                e.Property(r => r.Description).HasColumnName("Description").HasMaxLength(200);
                e.Property(r => r.CreatedAt).HasColumnName("CreatedAt");
                e.Property(r => r.UpdatedAt).HasColumnName("UpdatedAt");

                e.HasIndex(r => r.Name).IsUnique();
            });

            // ===== USER_ROLES (giữ nguyên)
            b.Entity<UserRole>(e =>
            {
                e.ToTable("user_roles");
                e.HasKey(ur => new { ur.UserId, ur.RoleId });

                e.Property(ur => ur.UserId).HasColumnName("UserId").IsRequired();
                e.Property(ur => ur.RoleId).HasColumnName("RoleId").IsRequired();

                e.HasIndex(ur => ur.UserId);
                e.HasIndex(ur => ur.RoleId);

                e.HasOne<User>().WithMany().HasForeignKey(ur => ur.UserId)
                    .HasConstraintName("fk_user_roles_user")
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne<Role>().WithMany().HasForeignKey(ur => ur.RoleId)
                    .HasConstraintName("fk_user_roles_role")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== CUSTOMERS (mới thêm)
            b.Entity<Customer>(e =>
            {
                e.ToTable("customers");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                e.Property(x => x.UserId).HasColumnName("user_id");                  // nullable
                e.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
                e.Property(x => x.Address).HasColumnName("address").HasMaxLength(500);
                e.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(32);
                e.Property(x => x.AddressProof).HasColumnName("address_proof").HasMaxLength(255);
                e.Property(x => x.CreatedAt).HasColumnName("created_at");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

                // Index/Unique theo schema
                e.HasIndex(x => x.UserId).IsUnique().HasDatabaseName("uk_customers_user");
                e.HasIndex(x => x.Phone).HasDatabaseName("idx_customers_phone");

                // FK -> users.id, ON DELETE SET NULL, ON UPDATE CASCADE
                e.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .HasConstraintName("fk_customers_user")
                 .OnDelete(DeleteBehavior.SetNull);
            });
            // ===== VEHICLES (mới thêm)
            b.Entity<Vehicle>(e =>
 {
     e.ToTable("vehicles");
     e.HasKey(x => x.Id);

     e.Property(x => x.Id)
         .HasColumnName("id")
         .ValueGeneratedOnAdd();

     e.Property(x => x.CustomerId)
         .HasColumnName("customer_id");

     e.Property(x => x.Name)
         .HasColumnName("name")
         .IsRequired()
         .HasMaxLength(255);

     e.Property(x => x.OwnerName)
         .HasColumnName("owner_name")
         .HasMaxLength(255);

     e.Property(x => x.Model)
         .HasColumnName("model")
         .HasMaxLength(100);

     e.Property(x => x.Version)
         .HasColumnName("version")
         .HasMaxLength(100);

     // ✅ Thêm cột seat_count thay cho rate
     e.Property(x => x.SeatCount)
         .HasColumnName("seat_count");

     e.Property(x => x.BodyNumber)
         .HasColumnName("body_number")
         .HasMaxLength(64);

     e.Property(x => x.EngineNumber)
         .HasColumnName("engine_number")
         .HasMaxLength(64);

     e.Property(x => x.VehicleNumber)
         .HasColumnName("vehicle_number")
         .HasMaxLength(64);

     e.Property(x => x.CreatedAt)
         .HasColumnName("created_at");

     e.Property(x => x.UpdatedAt)
         .HasColumnName("updated_at");

     // Index/Unique giống MySQL schema
     e.HasIndex(x => x.CustomerId).HasDatabaseName("idx_vehicles_customer");
     e.HasIndex(x => x.BodyNumber).IsUnique().HasDatabaseName("uk_vehicles_body");
     e.HasIndex(x => x.EngineNumber).IsUnique().HasDatabaseName("uk_vehicles_engine");
     e.HasIndex(x => x.VehicleNumber).IsUnique().HasDatabaseName("uk_vehicles_plate");

     // FK -> customers.id (ON DELETE SET NULL, ON UPDATE CASCADE)
     e.HasOne(x => x.Customer)
      .WithMany(c => c.Vehicles)
      .HasForeignKey(x => x.CustomerId)
      .HasConstraintName("fk_vehicles_customer")
      .OnDelete(DeleteBehavior.SetNull);
 });
            // ===== ESTIMATES (mới thêm)
            b.Entity<Estimate>(e =>
            {
                e.ToTable("estimates");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                e.Property(x => x.EstimateNumber).HasColumnName("estimate_number").IsRequired().HasMaxLength(32);

                e.Property(x => x.VehicleId).HasColumnName("vehicle_id"); // nullable

                e.Property(x => x.VehicleName).HasColumnName("vehicle_name").HasMaxLength(255);
                e.Property(x => x.VehicleModel).HasColumnName("vehicle_model").HasMaxLength(100);

                e.Property(x => x.Rate).HasColumnName("rate").HasColumnType("decimal(12,2)"); // nếu không dùng có thể bỏ
                e.Property(x => x.Warranty).HasColumnName("warranty").HasMaxLength(255);
                e.Property(x => x.PolicyType).HasColumnName("policy_type").HasMaxLength(100);

                e.Property(x => x.CreatedAt)
 .HasColumnName("created_at")
 .HasColumnType("timestamp")
 .ValueGeneratedOnAdd()
 .HasDefaultValueSql("CURRENT_TIMESTAMP");

                e.Property(x => x.UpdatedAt)
                 .HasColumnName("updated_at")
                 .HasColumnType("timestamp")
                 .ValueGeneratedOnAddOrUpdate()
                 .HasDefaultValueSql("CURRENT_TIMESTAMP");


                // Indexes
                e.HasIndex(x => x.EstimateNumber).IsUnique().HasDatabaseName("uk_estimates_no");
                e.HasIndex(x => x.VehicleId).HasDatabaseName("idx_estimates_vehicle");



                e.HasOne(x => x.Vehicle)
                    .WithMany()                 // nếu Vehicle chưa có ICollection<Estimate>, đổi thành .WithMany()
                    .HasForeignKey(x => x.VehicleId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("fk_estimates_vehicle");
            });

            // ===== POLICIES
            b.Entity<Policy>(e =>
            {
                e.ToTable("policies");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                e.Property(x => x.PolicyNumber).HasColumnName("policy_number").IsRequired().HasMaxLength(32);

                e.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();
                e.Property(x => x.VehicleId).HasColumnName("vehicle_id");

                e.Property(x => x.PolicyDate).HasColumnName("policy_date");
                e.Property(x => x.PolicyStartDate).HasColumnName("policy_start_date");
                e.Property(x => x.PolicyEndDate).HasColumnName("policy_end_date");
                e.Property(x => x.PolicyDurationDays).HasColumnName("policy_duration_days");

                // enum string <-> varchar (MySQL 5.7)
                e.Property(x => x.Status)
                 .HasColumnName("status")
                 .HasConversion<string>() // lưu string theo enum
                 .HasMaxLength(20)
                 .IsRequired();

                e.Property(x => x.VehicleNumber).HasColumnName("vehicle_number").HasMaxLength(64);
                e.Property(x => x.VehicleName).HasColumnName("vehicle_name").HasMaxLength(255);
                e.Property(x => x.VehicleModel).HasColumnName("vehicle_model").HasMaxLength(100);
                e.Property(x => x.VehicleVersion).HasColumnName("vehicle_version").HasMaxLength(100);
                e.Property(x => x.Rate).HasColumnName("rate").HasColumnType("decimal(12,2)");
                e.Property(x => x.Warranty).HasColumnName("warranty").HasMaxLength(255);
                e.Property(x => x.BodyNumber).HasColumnName("body_number").HasMaxLength(64);
                e.Property(x => x.EngineNumber).HasColumnName("engine_number").HasMaxLength(64);

                e.Property(x => x.CustomerAddress).HasColumnName("customer_address").HasMaxLength(500);
                e.Property(x => x.CustomerPhone).HasColumnName("customer_phone").HasMaxLength(32);
                e.Property(x => x.CustomerAddressProof).HasColumnName("customer_address_proof").HasMaxLength(255);

                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").ValueGeneratedOnAddOrUpdate().HasDefaultValueSql("CURRENT_TIMESTAMP");

                e.HasIndex(x => x.PolicyNumber).IsUnique().HasDatabaseName("uk_policies_no");
                e.HasIndex(x => x.CustomerId).HasDatabaseName("idx_policies_customer");
                e.HasIndex(x => x.VehicleId).HasDatabaseName("idx_policies_vehicle");
                e.HasIndex(x => x.Status).HasDatabaseName("idx_policies_status");

                e.HasOne<VehicleInsurance.Domain.Customers.Customer>()
                 .WithMany()
                 .HasForeignKey(x => x.CustomerId)
                 .HasConstraintName("fk_policies_customer")
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<VehicleInsurance.Domain.Entity.Vehicle>()
                 .WithMany()
                 .HasForeignKey(x => x.VehicleId)
                 .HasConstraintName("fk_policies_vehicle")
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ===== BILLINGS
            b.Entity<Billing>(e =>
            {
                e.ToTable("billings");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                e.Property(x => x.BillNo).HasColumnName("bill_no").IsRequired().HasMaxLength(32);
                e.Property(x => x.PolicyId).HasColumnName("policy_id").IsRequired();
                e.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();
                e.Property(x => x.VehicleId).HasColumnName("vehicle_id");
                e.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)").IsRequired();
                e.Property(x => x.BillDate).HasColumnName("bill_date").IsRequired();
                e.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
                e.Property(x => x.PaymentRef).HasColumnName("payment_ref").HasMaxLength(100);
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").ValueGeneratedOnAddOrUpdate().HasDefaultValueSql("CURRENT_TIMESTAMP");

                e.HasIndex(x => x.BillNo).IsUnique().HasDatabaseName("uk_billings_no");
                e.HasIndex(x => x.PolicyId).HasDatabaseName("idx_billings_policy");
                e.HasIndex(x => x.CustomerId).HasDatabaseName("idx_billings_customer");
                e.HasIndex(x => x.VehicleId).HasDatabaseName("idx_billings_vehicle");

                e.HasOne<VehicleInsurance.Domain.Policies.Policy>()
                 .WithMany()
                 .HasForeignKey(x => x.PolicyId)
                 .HasConstraintName("fk_billings_policy")
                 .OnDelete(DeleteBehavior.Restrict);
            });




        }
    }
}
