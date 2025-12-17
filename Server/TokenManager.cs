using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DoAn_NT106.Server
{
    public class TokenManager
    {
        private readonly string secretKey = "ThisIsAVeryStrongAndLongJwtSecretKey_2025!";
        private readonly string tokenFilePath = "tokens.json"; // file lưu token
        private ConcurrentDictionary<string, string> validTokens = new();

        public TokenManager()
        {
            LoadTokensFromFile();
        }

        // Generate token mới
        public string GenerateToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string jwt = tokenHandler.WriteToken(token);

            validTokens[jwt] = username;
            SaveTokensToFile(); // lưu vào file mỗi khi tạo token

            return jwt;
        }

        // Validate token
        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return validTokens.ContainsKey(token);
            }
            catch
            {
                return false;
            }
        }

        public string GetUsernameFromToken(string token)
        {
            return validTokens.ContainsKey(token) ? validTokens[token] : null;
        }

        public void RevokeToken(string token)
        {
            validTokens.TryRemove(token, out _);
            SaveTokensToFile();
        }

        // Lưu token vào file JSON
        private void SaveTokensToFile()
        {
            try
            {
                File.WriteAllText(tokenFilePath, JsonSerializer.Serialize(validTokens));
            }
            catch
            {
                // ignore lỗi ghi file
            }
        }

        // Load token từ file JSON
        private void LoadTokensFromFile()
        {
            if (!File.Exists(tokenFilePath)) return;

            try
            {
                string json = File.ReadAllText(tokenFilePath);
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (dict != null)
                {
                    validTokens = new ConcurrentDictionary<string, string>(dict);
                }
            }
            catch
            {
                // ignore lỗi đọc file
                validTokens = new ConcurrentDictionary<string, string>();
            }
        }
    }
}
