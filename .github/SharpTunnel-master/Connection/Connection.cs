using System;
using System.IO;
using System.Net;

namespace TA.UgconnectTunnel.Connection
{
	public abstract class NetConnection : IDisposable
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
