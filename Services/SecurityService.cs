using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace DoAn_NT106.Services
{
    public class SecurityService
    {
        // Lưu số lần đăng nhập sai
        private ConcurrentDictionary<string, (int Attempts, DateTime LastAttempt)> loginAttempts = new();

        // Thời gian khóa tài khoản (phút)
        private const int LOCKOUT_MINUTES = 5;

        // Số lần nhập sai tối đa
        private const int MAX_ATTEMPTS = 5;

        public bool CheckLoginAttempts(string username)
        {
            if (!loginAttempts.ContainsKey(username))
                return true;

            var (attempts, lastAttempt) = loginAttempts[username];

            // Kiểm tra nếu đã quá thời gian khóa
            if ((DateTime.Now - lastAttempt).TotalMinutes >= LOCKOUT_MINUTES)
            {
                loginAttempts.TryRemove(username, out _);
                return true;
            }

            // Nếu vẫn trong thời gian khóa và đã vượt số lần
            if (attempts >= MAX_ATTEMPTS)
            {
                int remainingMinutes = LOCKOUT_MINUTES - (int)(DateTime.Now - lastAttempt).TotalMinutes;
                return false; // Bị khóa
            }

            return true;
        }

        public void RecordLoginAttempt(string username, bool success)
        {
            if (success)
            {
                // Xóa record nếu đăng nhập thành công
                loginAttempts.TryRemove(username, out _);
                return;
            }

            // Tăng số lần thất bại
            if (!loginAttempts.ContainsKey(username))
            {
                loginAttempts[username] = (1, DateTime.Now);
            }
            else
            {
                var (attempts, _) = loginAttempts[username];
                loginAttempts[username] = (attempts + 1, DateTime.Now);
            }
        }

        public int GetRemainingAttempts(string username)
        {
            if (!loginAttempts.ContainsKey(username))
                return MAX_ATTEMPTS;

            var (attempts, _) = loginAttempts[username];
            return Math.Max(0, MAX_ATTEMPTS - attempts);
        }

        public int GetLockoutMinutes(string username)
        {
            if (!loginAttempts.ContainsKey(username))
                return 0;

            var (attempts, lastAttempt) = loginAttempts[username];

            if (attempts < MAX_ATTEMPTS)
                return 0;

            int elapsed = (int)(DateTime.Now - lastAttempt).TotalMinutes;
            return Math.Max(0, LOCKOUT_MINUTES - elapsed);
        }

       
    }
}