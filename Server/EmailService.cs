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

                string htmlBody = GenerateOtpEmailTemplate(otp);

                using var message = new MailMessage(senderEmail, toEmail)
                {
                    Subject = "🔐 OTP Code - Password Reset",
                    Body = htmlBody,
                    IsBodyHtml = true
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

        /// <summary>
        /// Generate beautiful HTML template for OTP email
        /// </summary>
        private string GenerateOtpEmailTemplate(string otp)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>OTP Verification Code</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}

        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 20px;
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
        }}

        .container {{
            max-width: 500px;
            width: 100%;
            background: white;
            border-radius: 12px;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
            overflow: hidden;
        }}

        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 30px 20px;
            text-align: center;
            color: white;
        }}

        .header-icon {{
            font-size: 48px;
            margin-bottom: 10px;
        }}

        .header h1 {{
            font-size: 28px;
            font-weight: 600;
            margin-bottom: 5px;
        }}

        .header p {{
            font-size: 14px;
            opacity: 0.9;
        }}

        .content {{
            padding: 40px 30px;
        }}

        .greeting {{
            font-size: 16px;
            color: #333;
            margin-bottom: 20px;
            line-height: 1.6;
        }}

        .otp-section {{
            background: #f8f9ff;
            border: 2px solid #667eea;
            border-radius: 8px;
            padding: 30px;
            margin: 25px 0;
            text-align: center;
        }}

        .otp-label {{
            font-size: 12px;
            color: #666;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 10px;
            font-weight: 600;
        }}

        .otp-code {{
            font-size: 48px;
            font-weight: 700;
            color: #667eea;
            letter-spacing: 8px;
            font-family: 'Courier New', monospace;
            margin: 15px 0;
        }}

        .otp-subtext {{
            font-size: 13px;
            color: #999;
            margin-top: 10px;
        }}

        .info-box {{
            background: #fff3cd;
            border-left: 4px solid #ffc107;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
        }}

        .info-box-title {{
            font-size: 12px;
            color: #856404;
            font-weight: 600;
            text-transform: uppercase;
            margin-bottom: 8px;
        }}

        .info-box-content {{
            font-size: 13px;
            color: #856404;
            line-height: 1.6;
        }}

        .security-tips {{
            background: #e8f4f8;
            border-left: 4px solid #17a2b8;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
        }}

        .security-tips-title {{
            font-size: 12px;
            color: #0c5460;
            font-weight: 600;
            text-transform: uppercase;
            margin-bottom: 8px;
        }}

        .security-tips-content {{
            font-size: 13px;
            color: #0c5460;
            line-height: 1.6;
        }}

        .security-tips ul {{
            margin-left: 15px;
            margin-top: 8px;
        }}

        .security-tips li {{
            margin-bottom: 6px;
        }}

        .footer {{
            background: #f8f9fa;
            padding: 20px 30px;
            text-align: center;
            border-top: 1px solid #e0e0e0;
        }}

        .footer-text {{
            font-size: 12px;
            color: #999;
            line-height: 1.6;
        }}

        .footer-link {{
            color: #667eea;
            text-decoration: none;
        }}

        .footer-link:hover {{
            text-decoration: underline;
        }}

        .timestamp {{
            font-size: 11px;
            color: #ccc;
            margin-top: 10px;
        }}

        @media (max-width: 600px) {{
            .container {{
                margin: 10px;
            }}

            .header {{
                padding: 20px 15px;
            }}

            .header h1 {{
                font-size: 24px;
            }}

            .content {{
                padding: 25px 20px;
            }}

            .otp-code {{
                font-size: 36px;
                letter-spacing: 4px;
            }}

            .otp-section {{
                padding: 20px;
            }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <!-- Header -->
        <div class='header'>
            <div class='header-icon'>🔐</div>
            <h1>Account Verification</h1>
            <p>Your OTP code is ready</p>
        </div>

        <!-- Content -->
        <div class='content'>
            <!-- Greeting -->
            <div class='greeting'>
                Hello,<br><br>
                You have requested to reset the password for your account. Please use the OTP code below to verify your identity and continue with the password recovery process.
            </div>

            <!-- OTP Section -->
            <div class='otp-section'>
                <div class='otp-label'>Your OTP Code</div>
                <div class='otp-code'>{otp}</div>
                <div class='otp-subtext'>✓ This code is valid for 5 minutes</div>
            </div>

            <!-- Info Box -->
            <div class='info-box'>
                <div class='info-box-title'>⏰ Important Information</div>
                <div class='info-box-content'>
                    This OTP code can only be used once. If you did not request this, please ignore this email.
                </div>
            </div>

            <!-- Security Tips -->
            <div class='security-tips'>
                <div class='security-tips-title'>🛡️ Security Notice</div>
                <div class='security-tips-content'>
                    <ul>
                        <li>NEVER share your OTP code with anyone</li>
                        <li>Support staff will never ask for your OTP code</li>
                        <li>Always check the browser URL before entering the code</li>
                    </ul>
                </div>
            </div>
        </div>

        <!-- Footer -->
        <div class='footer'>
            <div class='footer-text'>
                © {DateTime.Now.Year} Pixel Game Studio. All rights reserved.<br>
                <a href='#' class='footer-link'>Privacy Policy</a> | 
                <a href='#' class='footer-link'>Terms of Service</a><br>
                <span class='timestamp'>Sent at: {DateTime.Now:HH:mm:ss dd/MM/yyyy}</span>
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}
