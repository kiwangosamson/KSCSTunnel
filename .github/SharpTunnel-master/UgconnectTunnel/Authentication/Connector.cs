using System;
using System.IO;

namespace TA.UgconnectTunnel.ModuleBaseClasses
{
	public abstract class ClientConnector : IDisposable
    {
        public abstract Stream Stream { get; }
        
		public abstract void Connect();
		public abstract void Disconnect();
		
    	public virtual void Dispose()
    	{
    		try { Disconnect(); }
    		catch { }
    	}
    }

    public abstract class ServerConnector : IDisposable
    {
        public abstract Stream Stream { get; }
        
    	public abstract void Connect();
		public abstract void Disconnect();

    	public virtual void Dispose()
    	{
    		try { Disconnect(); }
    		catch { }
    	}
    }
}
