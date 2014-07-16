using System;
using System.IO;
using System.Net;

namespace TA.SharpTunnel.Connections
{
	public abstract class Connection : IDisposable
	{
		public abstract Stream Stream { get; }
		public abstract IPEndPoint EndPoint { get; }
		
        public abstract void Connect();
        public abstract void Disconnect();

        public virtual void Dispose()
        {
            try { Disconnect(); }
            catch { }
        }
	}
}
