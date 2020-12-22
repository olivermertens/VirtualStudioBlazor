using System.Collections.Generic;
using System.Net;

namespace VirtualStudio.Shared
{
    public struct ConnectionPoint
    {
        public string Ip { get; set; }
        public int Port { get; set; }

        public ConnectionPoint(string ip, int port)
        {
            Ip = ip;
            Port = port;
        }
    }

    public class NetworkInfo
    {
        public ConnectionPoint LocalEndpoint { get; set; }
        public List<ConnectionPoint> IceCandidates { get; set; }
    }
}