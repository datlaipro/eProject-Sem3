namespace VehicleInsurance.Infrastructure
{
    using Microsoft.EntityFrameworkCore;
    using VehicleInsurance.Domain.Users;
    using VehicleInsurance.Domain.Auth;
    using VehicleInsurance.Domain.EmailVerification;
    using VehicleInsurance.Domain.Roles; // <- thêm

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

        // DbSets
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
        public DbSet<Role> Roles => Set<Role>();         // <- thêm
        public DbSet<UserRole> UserRoles => Set<UserRole>(); // <- thêm

        protected override void OnModelCreating(ModelBuilder b)
        {
            // ===== USERS
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

            // ===== REFRESH TOKENS
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

                // UNIQUE đúng theo DB hiện tại
                e.HasIndex(x => x.TokenHash).IsUnique();

                e.HasIndex(x => x.ExpiresAt);
                e.HasIndex(x => x.Revoked);
            });

            // ===== EMAIL VERIFICATION TOKENS
            b.Entity<EmailVerificationToken>(e =>
            {
                e.ToTable("email_verification_tokens");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
                e.Property(x => x.Token).HasColumnName("token").IsRequired().HasMaxLength(255);

                // map cột DB (không có hậu tố _utc)
                e.Property(x => x.ExpiresAtUtc).HasColumnName("expires_at").IsRequired();
                e.Property(x => x.UsedAtUtc).HasColumnName("used_at");
                e.Property(x => x.CreatedAtUtc).HasColumnName("created_at").IsRequired();

                e.HasIndex(x => x.Token).IsUnique();
                e.HasIndex(x => new { x.UserId, x.UsedAtUtc });
            });

            // ===== ROLES
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

            // ===== USER_ROLES
            b.Entity<UserRole>(e =>
            {
                e.ToTable("user_roles");
                e.HasKey(ur => new { ur.UserId, ur.RoleId });

                e.Property(ur => ur.UserId).HasColumnName("UserId").IsRequired();
                e.Property(ur => ur.RoleId).HasColumnName("RoleId").IsRequired();

                e.HasIndex(ur => ur.UserId);
                e.HasIndex(ur => ur.RoleId);
            });
        }
    }
}
