using System;

namespace DoAn_NT106.Client
{
    /// <summary>
    /// Centralized configuration for server connections (TCP & UDP)
    /// </summary>
    public static class AppConfig
    {
        // ? SERVER IP/PORT - Single source of truth for all connections
        public const string SERVER_IP = "103.188.244.112";    
        public const int TCP_PORT = 8080;               // ? TCP Server port
        public const int UDP_PORT = 5000;               // ? UDP Game Server port

        /// <summary>
        /// Validate configuration
        /// </summary>
        public static (bool IsValid, string Message) Validate()
        {
            if (string.IsNullOrWhiteSpace(SERVER_IP))
                return (false, "SERVER_IP is not configured");

            if (TCP_PORT <= 0 || TCP_PORT > 65535)
                return (false, $"Invalid TCP_PORT: {TCP_PORT}");

            if (UDP_PORT <= 0 || UDP_PORT > 65535)
                return (false, $"Invalid UDP_PORT: {UDP_PORT}");

            return (true, "Configuration is valid");
        }

        /// <summary>
        /// Get connection info for logging
        /// </summary>
        public static string GetConnectionInfo()
        {
            return $"Server: {SERVER_IP} | TCP: {TCP_PORT} | UDP: {UDP_PORT}";
        }
    }
}
