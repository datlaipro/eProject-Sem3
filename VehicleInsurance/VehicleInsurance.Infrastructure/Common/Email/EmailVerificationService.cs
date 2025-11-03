// File: VehicleInsurance.Infrastructure/Common/Email/EmailVerificationService.cs
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using VehicleInsurance.Domain.Common.Email;
using VehicleInsurance.Application.EmailVerification;
using VehicleInsurance.Domain.Common.Errors;
using VehicleInsurance.Domain.Common.Exceptions;
using VehicleInsurance.Infrastructure.Data;
using VehicleInsurance.Domain.Users;
using VehicleInsurance.Domain.EmailVerification;
using VehicleInsurance.Infrastructure; // AppDbContext

namespace VehicleInsurance.Infrastructure.Common.Email;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly IEmailVerificationTokenRepository _repo;
    private readonly IEmailSender _email;
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;
    private readonly ILogger<EmailVerificationService> _log;

    public EmailVerificationService(
        IEmailVerificationTokenRepository repo,
        IEmailSender email,
        AppDbContext db,
        IConfiguration cfg,
        ILogger<EmailVerificationService> log)
    {
        _repo = repo;
        _email = email;
        _db = db;
        _cfg = cfg;
        _log = log;
    }

    /// <summary>
    /// Nhận 1 trong 3 tiêu chí: userId hoặc email hoặc username.
    /// </summary>
    public async Task SendVerificationAsync(
        long? userId = null,
        string? email = null,
        string? username = null,
        CancellationToken ct = default)
    {
        // 1) Ít nhất 1 tiêu chí
        if (userId is null && string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(username))
            throw new BadRequestAppException("Cần cung cấp userId hoặc email hoặc username.", ErrorCodes.VerifyCriteriaMissing);

        // 2) Lấy user theo tiêu chí
        var user = await ResolveUserAsync(userId, email, username, ct);
        if (user is null)
            throw new NotFoundException("Không tìm thấy người dùng.", ErrorCodes.VerifyUserNotFound);

        // Nếu truyền nhiều tiêu chí mà không khớp cùng user → 400
        EnsureIdentityConsistencyOrThrow(user, userId, email, username);

        // 3) Kiểm tra đã xác minh chưa (dùng User.EmailConfirmed)
        if (user.EmailConfirmed)
            throw new ConflictException("Email đã được xác minh.", ErrorCodes.VerifyAlreadyDone);

        // 4) Sinh token URL-safe
        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

        // 5) Lưu token trước khi gửi mail
        var record = new EmailVerificationToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(24)
        };
        await _repo.SaveAsync(record, ct);

        // 6) Tạo link verify
        var baseVerify = _cfg["Email:VerifyBaseUrl"];
        string verifyUrl = !string.IsNullOrWhiteSpace(baseVerify)
            ? $"{baseVerify.TrimEnd('/')}?token={Uri.EscapeDataString(token)}"
            : $"http://localhost:5155/api/verify-email?token={Uri.EscapeDataString(token)}";

        // 7) Gửi email
        var displayName = string.IsNullOrWhiteSpace(user.Username) ? "bạn" : user.Username;
        var html = $@"
<h3>Xin chào {System.Net.WebUtility.HtmlEncode(displayName)}</h3>
<p>Vui lòng xác nhận email cho tài khoản VehicleInsurance.</p>
<p><a href=""{verifyUrl}"">Nhấn vào đây để xác nhận</a></p>
<p>Liên kết sẽ hết hạn sau 24 giờ.</p>";

        _log.LogInformation("SendVerification: user {Id} <{Email}>; link={Url}", user.Id, user.Email, verifyUrl);
        await _email.SendAsync(user.Email, "Xác nhận email tài khoản", html, ct);
    }

    /// <summary>
    /// Back-compat: chữ ký cũ gọi sang hàm mới.
    /// </summary>
    public Task SendVerificationAsync(long userId, string email, string username, CancellationToken ct = default)
        => SendVerificationAsync(userId: userId, email: email, username: username, ct: ct);

    /// <summary>
    /// Xác minh qua token. Thành công trả true, ngược lại ném AppException để middleware map status code.
    /// </summary>
    public async Task<bool> VerifyAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new BadRequestAppException("Thiếu token.", ErrorCodes.BadRequest);

        // Tìm token
        var record = await _repo.FindByTokenAsync(token, ct);
        if (record is null || record.UsedAtUtc is not null || record.ExpiresAtUtc < DateTime.UtcNow)
            throw new GoneAppException("Token không hợp lệ hoặc đã hết hạn/đã dùng.", ErrorCodes.VerifyTokenInvalid);

        // Lấy user kèm kiểm tra tồn tại
        var user = await _db.Set<User>().FirstOrDefaultAsync(u => u.Id == record.UserId, ct);
        if (user is null)
            throw new GoneAppException("Token không hợp lệ hoặc đã hết hạn/đã dùng.", ErrorCodes.VerifyTokenInvalid);

        // Nếu đã xác minh rồi (có thể do chạy verify 2 lần)
        if (user.EmailConfirmed)
        {
            // Đánh dấu token đã dùng nếu chưa
            if (record.UsedAtUtc is null)
                await _repo.MarkUsedAsync(record.Id, ct);

            throw new GoneAppException("Token không hợp lệ hoặc đã hết hạn/đã dùng.", ErrorCodes.VerifyTokenInvalid);
        }

        // Transaction: set EmailConfirmed + mark token used
        using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            user.EmailConfirmed = true;
            _db.Update(user);
            await _db.SaveChangesAsync(ct);

            await _repo.MarkUsedAsync(record.Id, ct);

            await tx.CommitAsync(ct);
            _log.LogInformation("Verify: success for user {UserId} (tokenId={TokenId})", user.Id, record.Id);
            return true;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw; // để middleware xử lý 500 nếu có sự cố
        }
    }

    // ================== Helpers ==================

    private async Task<User?> ResolveUserAsync(long? userId, string? email, string? username, CancellationToken ct)
    {
        User? byId = null, byEmail = null, byUsername = null;

        if (userId is not null && userId > 0)
            byId = await _db.Set<User>().FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (!string.IsNullOrWhiteSpace(email))
            byEmail = await _db.Set<User>().FirstOrDefaultAsync(u => u.Email == email, ct);

        if (!string.IsNullOrWhiteSpace(username))
            byUsername = await _db.Set<User>().FirstOrDefaultAsync(u => u.Username == username, ct);

        return byId ?? byEmail ?? byUsername;
    }

    private static void EnsureIdentityConsistencyOrThrow(User chosen, long? userId, string? email, string? username)
    {
        if (userId is not null && userId > 0 && chosen.Id != userId.Value)
            throw new BadRequestAppException("userId không khớp với thông tin còn lại.", ErrorCodes.VerifyMismatch);

        if (!string.IsNullOrWhiteSpace(email) &&
            !string.Equals(chosen.Email, email, StringComparison.OrdinalIgnoreCase))
            throw new BadRequestAppException("email không khớp với thông tin còn lại.", ErrorCodes.VerifyMismatch);

        if (!string.IsNullOrWhiteSpace(username) &&
            !string.Equals(chosen.Username, username, StringComparison.Ordinal))
            throw new BadRequestAppException("username không khớp với thông tin còn lại.", ErrorCodes.VerifyMismatch);
    }
}
