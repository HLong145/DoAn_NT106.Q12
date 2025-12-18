using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn_NT106.Client.Class
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Player { get; set; }
        public string Message { get; set; }
        public string Time { get; set; }
    }

    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public bool IsReady { get; set; }
    }
}