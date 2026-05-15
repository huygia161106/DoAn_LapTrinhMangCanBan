using System.IO;

namespace Server
{
    public class AgentSession
    {
        public string ShareCode { get; set; }

        public string MachineName { get; set; }

        public string IP { get; set; }

        public StreamWriter Writer { get; set; }

        public string LatestData { get; set; }
    }
}