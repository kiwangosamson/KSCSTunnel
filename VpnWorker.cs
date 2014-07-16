using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using TA.SharpTunnel.TunTap;
using TA.SharpTunnel.Connections;
//Remove some usings

namespace TA.SharpTunnel
{
    public class VpnWorker
    {
        private Connection _Connection;
        private TunTapDevice _TunTap;

        public VpnWorker(Connection Connection, TunTapDevice TunTap)
        {
            _Connection = Connection;
            _TunTap = TunTap;
        }
    }
}
