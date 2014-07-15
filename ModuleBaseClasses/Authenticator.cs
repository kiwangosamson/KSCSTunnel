using System;
using System.Net;

namespace TA.SharpTunnel.ModuleBaseClasses
{
	public abstract class ClientAuthenticator : IDisposable
    {
    	public abstract byte[] CreateResponse(byte[] Request);
    	
    	public virtual void Dispose() { }
    }

	public abstract class ServerAuthenticator : IDisposable
    {
    	public abstract byte[] CreateRequest(IPEndPoint Client);
    	public abstract byte[] CheckResponse(IPEndPoint Client, byte[] Request, byte[] Response);
    	
    	public virtual void Dispose() { }
    }
}
