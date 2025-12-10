using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DoAn_NT106.Server
{
    public class EmailService
    {
        private readonly string senderEmail;
        private readonly string senderAppPassword;

        // Nhận email + app password từ ngoài truyền vào
        public EmailService(string email, string appPassword)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("sender email is required", nameof(email));

            senderEmail = email;
            senderAppPassword = appPassword ?? string.Empty;
        }

        public void SendOtp(string toEmail, string otp)
        {
            // synchronous wrapper
            SendOtpAsync(toEmail, otp).GetAwaiter().GetResult();
        }

        public async Task SendOtpAsync(string toEmail, string otp)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("recipient email is required", nameof(toEmail));

            // basic validation
            try
            {
                var _ = new MailAddress(toEmail);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid recipient email format", nameof(toEmail), ex);
            }

            try
            {
                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(senderEmail, senderAppPassword)
                };

                using var message = new MailMessage(senderEmail, toEmail)
                {
                    Subject = "Mã OTP khôi phục mật khẩu",
                    Body =
                        $"Xin chào,\n\n" +
                        $"Mã OTP của bạn là: {otp}\n" +
                        $"OTP có hiệu lực trong 5 phút.\n\n" +
                        $"Vui lòng KHÔNG chia sẻ mã này cho bất kỳ ai.",
                    IsBodyHtml = false
                };

                await smtp.SendMailAsync(message).ConfigureAwait(false);

                Console.WriteLine("📧 OTP sent to " + toEmail);
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine("❌ Email send error: " + smtpEx.Message);
                throw new Exception("Unable to send OTP email. Please check SMTP configuration.", smtpEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Email send error: " + ex.Message);
                throw;
            }
        }
    }
}
