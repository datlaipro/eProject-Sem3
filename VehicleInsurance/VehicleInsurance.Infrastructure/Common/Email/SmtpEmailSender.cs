using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using VehicleInsurance.Application.Common.Email;

namespace VehicleInsurance.Infrastructure.Common.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _user;
    private readonly string _pass;
    private readonly string _from;

    public SmtpEmailSender(string host, int port, string user, string pass, string from)
    {
        _host = host;
        _port = port;
        _user = user;
        _pass = pass;
        _from = from;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var msg = new MimeMessage();
        msg.From.Add(MailboxAddress.Parse(_from));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody, TextBody = StripHtml(htmlBody) };
        msg.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls, ct);
        await client.AuthenticateAsync(_user, _pass, ct);
        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);
    }

    private static string StripHtml(string html) =>
        System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
}
