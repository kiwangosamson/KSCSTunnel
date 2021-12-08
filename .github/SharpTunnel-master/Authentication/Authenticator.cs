using System;
using System.Net;

namespace TA.UgconnectTunnel.Authentication
{
    public abstract class Authenticator : IDisposable
    {
        public abstract byte[] ServerCreateChallenge(IPEndPoint Client);
        public abstract byte[] ClientCreateResponse(IPEndPoint Server, byte[] Challenge);
        public abstract bool ServerCheckResponse(IPEndPoint Client, byte[] Challenge, byte[] Response);
        
        public virtual void Dispose() { }
    }
}
