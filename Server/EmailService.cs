using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace DoAn_NT106.Server
{
    public class EmailService
    {
        private readonly string senderEmail;
        private readonly string senderAppPassword;

        // 👉 Nhận email + app password từ ngoài truyền vào
        public EmailService(string email, string appPassword)
        {
            senderEmail = email;
            senderAppPassword = appPassword;
        }

        public void SendOtp(string toEmail, string otp)
        {
            try
            {
                var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(senderEmail, senderAppPassword),
                    EnableSsl = true
                };

                var message = new MailMessage(senderEmail, toEmail)
                {
                    Subject = "Mã OTP khôi phục mật khẩu",
                    Body =
                        $"Xin chào,\n\n" +
                        $"Mã OTP của bạn là: {otp}\n" +
                        $"OTP có hiệu lực trong 5 phút.\n\n" +
                        $"Vui lòng KHÔNG chia sẻ mã này cho bất kỳ ai.",
                    IsBodyHtml = false
                };

                smtp.Send(message);

                Console.WriteLine("📧 OTP sent to " + toEmail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Email send error: " + ex.Message);
                throw new Exception("Unable to send OTP email. Please check SMTP configuration.");
            }
        }
    }
}
