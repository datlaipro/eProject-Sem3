namespace VehicleInsurance.Domain.EmailVerification;

public class EmailVerificationToken
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Token { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? UsedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
