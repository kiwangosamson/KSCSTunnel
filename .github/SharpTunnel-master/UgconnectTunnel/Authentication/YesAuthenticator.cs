using System;
using System.Net;

namespace TA.UgconnectTunnel.Authentication
{
    public class YesAuthenticator : Authenticator
    {
        public override byte[] ServerCreateChallenge(IPEndPoint Client) 
        {
            var challenge = new byte[1];
            (new Random()).NextBytes(challenge);
            return challenge;
        }
        
        public override byte[] ClientCreateResponse(IPEndPoint Server, byte[] Challenge)
        {
            if (Challenge.Length != 1)
                return new byte[1] { 0x00 };
            else return new byte[1] { (byte)(255 - Challenge[0]) };
        }
        
        public override bool ServerCheckResponse(IPEndPoint Client, byte[] Challenge, byte[] Response)
        {
            if (Challenge.Length != 1 || Response.Length != 1)
                return false;
            
            return (255 - Response[0]) == Challenge[0];
        }
    }
}
