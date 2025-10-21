using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleInsurance.Domain.EmailVerification;

namespace VehicleInsurance.Infrastructure.EmailVerification;

public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> b)
    {
        b.ToTable("email_verification_tokens");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();

        b.Property(x => x.UserId).IsRequired();
        b.Property(x => x.Token).HasMaxLength(255).IsRequired();
        b.Property(x => x.ExpiresAtUtc).IsRequired();
        b.Property(x => x.UsedAtUtc);
        b.Property(x => x.CreatedAtUtc).IsRequired();

        b.HasIndex(x => x.Token).IsUnique();
        b.HasIndex(x => new { x.UserId, x.UsedAtUtc });
    }
}
