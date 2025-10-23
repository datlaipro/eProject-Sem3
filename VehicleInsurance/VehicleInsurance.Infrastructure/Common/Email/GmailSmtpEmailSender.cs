using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using VehicleInsurance.Application.Common.Email;

namespace VehicleInsurance.Infrastructure.Common.Email;

public class GmailSmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _cfg;
    public GmailSmtpEmailSender(IConfiguration cfg) => _cfg = cfg;

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        var from = _cfg["Email:From"] ?? throw new InvalidOperationException("Missing Email:From");
        var display = _cfg["Email:DisplayName"] ?? from;

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(display, from));
        msg.To.Add(MailboxAddress.Parse(toEmail));
        msg.Subject = subject;
        msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        // Ghi transcript SMTP ra file tạm (DEV)
        var logPath = Path.Combine(Path.GetTempPath(), $"smtp-{DateTime.Now:HHmmss}.log");
        using var logger = new ProtocolLogger(logPath);
        using var smtp = new SmtpClient(logger);

        smtp.AuthenticationMechanisms.Remove("XOAUTH2");

        var host = _cfg["Email:Smtp:Host"] ?? "smtp.gmail.com";
        var port = int.TryParse(_cfg["Email:Smtp:Port"], out var p) ? p : 587;
        var secure = port == 587 ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect;

        Console.WriteLine($"[SMTP] host={host}, port={port}, mode={secure}, log={logPath}");

        try
        {
            await smtp.ConnectAsync(host, port, secure, ct);
            Console.WriteLine($"[SMTP] connected={smtp.IsConnected}, capabilities={smtp.Capabilities}");

            if (!smtp.IsConnected)
                throw new InvalidOperationException("SMTP not connected after ConnectAsync.");

            var user = _cfg["Email:Smtp:User"]!;
            Console.WriteLine($"[SMTP] authenticating as {user}");
            await smtp.AuthenticateAsync(user, _cfg["Email:Smtp:AppPassword"]!, ct);
            await smtp.SendAsync(msg, ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[SMTP] EX: " + ex);   // log full stack
            throw; // để middleware trả lỗi
        }
        finally
        {
            if (smtp.IsConnected)
                await smtp.DisconnectAsync(true, ct);
        }
    }
}
