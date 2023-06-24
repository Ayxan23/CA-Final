namespace CAFinal.Utils
{
    public class EmailHelper
    {
        public async Task SendEmailAsync(MailRequestViewModel mailRequestViewModel)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(Constants.mail);
            email.To.Add(MailboxAddress.Parse(mailRequestViewModel.ToEmail));
            email.Subject = mailRequestViewModel.Subject;
            var builder = new BodyBuilder();

            builder.HtmlBody = mailRequestViewModel.Body;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(Constants.host, Constants.port, SecureSocketOptions.StartTls);
            smtp.Authenticate(Constants.mail, Constants.password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
    }
}
