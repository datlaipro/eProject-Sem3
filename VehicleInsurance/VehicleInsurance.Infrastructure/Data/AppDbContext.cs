using Microsoft.EntityFrameworkCore;
using VehicleInsurance.Domain.Auth;
using VehicleInsurance.Domain.Billings;
using VehicleInsurance.Domain.Customers;
using VehicleInsurance.Domain.EmailVerification;
using VehicleInsurance.Domain.Entity;

using VehicleInsurance.Domain.Policies;
using VehicleInsurance.Domain.Roles;
using VehicleInsurance.Domain.Users;

namespace VehicleInsurance.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

        // ===================== DbSets =====================
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<Estimate> Estimates => Set<Estimate>();
        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<Billing> Billings => Set<Billing>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();

        // ===================== OnModelCreating =====================
        protected override void OnModelCreating(ModelBuilder b)
        {
            // ===== USERS =====
            b.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                e.Property(x => x.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
                e.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
                e.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
                e.Property(x => x.Role).HasColumnName("role").HasMaxLength(32).IsRequired();
                e.Property(x => x.Active).HasColumnName("active");
                e.HasIndex(x => x.Username).IsUnique();
                e.HasIndex(x => x.Email).IsUnique();
            });

            // ===== ROLES =====
            b.Entity<Role>(e =>
            {
                e.ToTable("roles");
                e.HasKey(x => x.RoleId);
                e.Property(x => x.RoleId).HasColumnName("RoleId").ValueGeneratedOnAdd();
                e.Property(x => x.Name).HasColumnName("Name").HasMaxLength(50).IsRequired();
                e.Property(x => x.Description).HasColumnName("Description").HasMaxLength(200);
                e.Property(x => x.CreatedAt).HasColumnName("CreatedAt");
                e.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt");
                e.HasIndex(x => x.Name).IsUnique();
            });
            // ===== REFRESH TOKENS =====
            b.Entity<RefreshToken>(e =>
            {
                e.ToTable("refresh_tokens");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
                e.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(64).IsRequired();
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


            // ===== USER_ROLES =====
            b.Entity<UserRole>(e =>
            {
                e.ToTable("user_roles");
                e.HasKey(x => new { x.UserId, x.RoleId });
                e.Property(x => x.UserId).HasColumnName("UserId");
                e.Property(x => x.RoleId).HasColumnName("RoleId");

                e.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasConstraintName("fk_user_roles_user")
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne<Role>()
                    .WithMany()
                    .HasForeignKey(x => x.RoleId)
                    .HasConstraintName("fk_user_roles_role")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== CUSTOMERS =====
            b.Entity<Customer>(e =>
            {
                e.ToTable("customers");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                e.Property(x => x.UserId).HasColumnName("user_id");
                e.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
                e.Property(x => x.Address).HasColumnName("address").HasMaxLength(500);
                e.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(32);
                e.Property(x => x.AddressProof).HasColumnName("address_proof").HasMaxLength(255);
                e.Property(x => x.CreatedAt).HasColumnName("created_at");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

                e.HasIndex(x => x.UserId).IsUnique().HasDatabaseName("uk_customers_user");
                e.HasIndex(x => x.Phone).HasDatabaseName("idx_customers_phone");

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasConstraintName("fk_customers_user")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ===== VEHICLES =====
            b.Entity<Vehicle>(e =>
            {
                e.ToTable("vehicles");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                e.Property(x => x.CustomerId).HasColumnName("customer_id");
                e.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
                e.Property(x => x.OwnerName).HasColumnName("owner_name").HasMaxLength(255);
                e.Property(x => x.Model).HasColumnName("model").HasMaxLength(100);
                e.Property(x => x.Version).HasColumnName("version").HasMaxLength(100);
                e.Property(x => x.SeatCount).HasColumnName("seat_count");
                e.Property(x => x.BodyNumber).HasColumnName("body_number").HasMaxLength(64);
                e.Property(x => x.EngineNumber).HasColumnName("engine_number").HasMaxLength(64);
                e.Property(x => x.VehicleNumber).HasColumnName("vehicle_number").HasMaxLength(64);
                e.Property(x => x.CreatedAt).HasColumnName("created_at");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

                e.HasIndex(x => x.CustomerId).HasDatabaseName("idx_vehicles_customer");
                e.HasIndex(x => x.BodyNumber).IsUnique().HasDatabaseName("uk_vehicles_body");
                e.HasIndex(x => x.EngineNumber).IsUnique().HasDatabaseName("uk_vehicles_engine");
                e.HasIndex(x => x.VehicleNumber).IsUnique().HasDatabaseName("uk_vehicles_plate");

                e.HasOne(x => x.Customer)
                    .WithMany(c => c.Vehicles)
                    .HasForeignKey(x => x.CustomerId)
                    .HasConstraintName("fk_vehicles_customer")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ===== ESTIMATES =====
            b.Entity<Estimate>(e =>
 {
     e.ToTable("estimates");
     e.HasKey(x => x.Id);

     e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
     e.Property(x => x.EstimateNumber).HasColumnName("estimate_number").HasMaxLength(32).IsRequired();

     // ✅ Cho phép null
     e.Property(x => x.CustomerId).HasColumnName("customer_id");
     e.Property(x => x.CustomerPhone).HasColumnName("customer_phone").HasMaxLength(32);
     e.Property(x => x.VehicleId).HasColumnName("vehicle_id");
     e.Property(x => x.VehicleName).HasColumnName("vehicle_name").HasMaxLength(255);
     e.Property(x => x.VehicleModel).HasColumnName("vehicle_model").HasMaxLength(100);
     e.Property(x => x.Rate).HasColumnName("rate").HasColumnType("decimal(12,2)");
     e.Property(x => x.Warranty).HasColumnName("warranty").HasMaxLength(255);
     e.Property(x => x.PolicyType).HasColumnName("policy_type").HasMaxLength(100);

     e.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PENDING");
     e.Property(x => x.IsPublic).HasColumnName("is_public").HasDefaultValue(false);

     e.Property(x => x.CreatedAt).HasColumnName("created_at")
         .HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
     e.Property(x => x.UpdatedAt).HasColumnName("updated_at")
         .HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

     e.HasIndex(x => x.EstimateNumber).IsUnique().HasDatabaseName("uk_estimates_no");
     e.HasIndex(x => x.CustomerId).HasDatabaseName("idx_estimates_customer");
     e.HasIndex(x => x.VehicleId).HasDatabaseName("idx_estimates_vehicle");

     e.HasOne(x => x.Customer)
         .WithMany()
         .HasForeignKey(x => x.CustomerId)
         .HasConstraintName("fk_estimates_customer")
         .OnDelete(DeleteBehavior.SetNull);

     e.HasOne(x => x.Vehicle)
         .WithMany()
         .HasForeignKey(x => x.VehicleId)
         .HasConstraintName("fk_estimates_vehicle")
         .OnDelete(DeleteBehavior.SetNull);
 });


            // ===== POLICIES ===== (giữ nguyên logic trước)
            b.Entity<Policy>(e =>
            {
                e.ToTable("policies");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                e.Property(x => x.PolicyNumber).HasColumnName("policy_number").HasMaxLength(32).IsRequired();
                e.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();
                e.Property(x => x.VehicleId).HasColumnName("vehicle_id");
                e.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
                e.Property(x => x.Rate).HasColumnName("rate").HasColumnType("decimal(12,2)");
                e.Property(x => x.Warranty).HasColumnName("warranty").HasMaxLength(255);
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

                e.HasOne<Customer>()
                    .WithMany()
                    .HasForeignKey(x => x.CustomerId)
                    .HasConstraintName("fk_policies_customer")
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Vehicle>()
                    .WithMany()
                    .HasForeignKey(x => x.VehicleId)
                    .HasConstraintName("fk_policies_vehicle")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ===== BILLINGS ===== (giữ nguyên)
            b.Entity<Billing>(e =>
            {
                e.ToTable("billings");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                e.Property(x => x.BillNo).HasColumnName("bill_no").HasMaxLength(32).IsRequired();
                e.Property(x => x.PolicyId).HasColumnName("policy_id").IsRequired();
                e.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();
                e.Property(x => x.VehicleId).HasColumnName("vehicle_id");
                e.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)").IsRequired();
                e.Property(x => x.BillDate).HasColumnName("bill_date").IsRequired();
                e.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
                e.Property(x => x.PaymentRef).HasColumnName("payment_ref").HasMaxLength(100);
                e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

                e.HasOne<Policy>()
                    .WithMany()
                    .HasForeignKey(x => x.PolicyId)
                    .HasConstraintName("fk_billings_policy")
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
