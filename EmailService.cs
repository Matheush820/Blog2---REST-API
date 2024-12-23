using System.Net.Mail;
using System.Net;

public class EmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
        var smtpConfig = configuration.GetSection("SmtpConfiguration");
        _smtpHost = smtpConfig["SmtpHost"];
        _smtpPort = int.Parse(smtpConfig["SmtpPort"]);
        _smtpUsername = smtpConfig["SmtpUsername"];
        _smtpPassword = smtpConfig["SmtpPassword"];
        _fromEmail = smtpConfig["FromEmail"];
        _fromName = smtpConfig["FromName"];
    }

    public bool SendEmail(string toEmail, string subject, string body)
    {
        var client = new SmtpClient(_smtpHost, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            EnableSsl = true
        };

        var mail = new MailMessage
        {
            From = new MailAddress(_fromEmail, _fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(new MailAddress(toEmail));

        try
        {
            client.Send(mail);
            return true; // Sucesso
        }
        catch (Exception ex)
        {
            // Log o erro aqui se necessário
            return false; // Falha
        }
    }
}
