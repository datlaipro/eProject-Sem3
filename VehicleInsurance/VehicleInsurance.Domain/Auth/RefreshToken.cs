namespace VehicleInsurance.Domain.Auth;

public class RefreshToken
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string TokenHash { get; set; } = default!; // SHA-256 hex (64)
    public string? TokenFamily { get; set; }          // optional UUID
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool Revoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
