using System;
using System.Security;
using System.Collections.Generic;
using TA.UgconnectTunnel.TunTap;
using TA.UgconnectTunnel.Connection;
using TA.UgconnectTunnel.Authentication;

namespace TA.UgconnectTunnel
{
    public class VpnWorker
    {
        private List<Authenticator> _Authenticators;
        private NetConnection _Connection;
        private TunTapDevice _TunTap;

        public VpnWorker(NetConnection Connection, TunTapDevice TunTap, List<Authenticator> Authenticators)
        {
            _Authenticators = Authenticators;
            _Connection = Connection;
            _TunTap = TunTap;
        }
        
        public void HandleAsServer()
        {
            using (ReaderWriter rw = new ReaderWriter(_Connection.Stream))
            {
                //Send client all authenticators
                rw.Write((ushort)_Authenticators.Count);
                foreach (Authenticator av_auth in _Authenticators)
                    rw.Write(av_auth.GetType().FullName, true);
                rw.Flush();
                
                //Get wanted authenticator index
                ushort index = rw.ReadUShort();
                if (index == ushort.MaxValue || index > _Authenticators.Count - 1)
                    throw new SecurityException("Client can not authenticate");
                Authenticator auth = _Authenticators[index];
                
                //Send challenge
                byte[] challenge = auth.ServerCreateChallenge(_Connection.EndPoint);
                rw.Write(challenge, true);
                
                //Read response
                byte[] response = rw.ReadBytes();
                
                //Send result
                bool success = auth.ServerCheckResponse(_Connection.EndPoint, challenge, response);
                rw.Write(success);
                
                if (success)
                    HandleAuthenticatedConnection();
                else throw new SecurityException("Client authentication failed");
            }
        }
        
        public void HandleAsClient()
        {
            using (ReaderWriter rw = new ReaderWriter(_Connection.Stream))
            {
                //Read server authenticators
                ushort count = rw.ReadUShort();
                var authenticators = new string[count];
                for (ushort i = 0; i < count; i++)
                    authenticators[i] = rw.ReadString();
                
                //Select the best authenticator
                //TODO Authenticator select
                Type authType = Type.GetType(authenticators[0], false, false);
                object authInstance = null;
                try { authInstance = Activator.CreateInstance(authType); }
                catch (Exception ex)
                {
                    rw.Write(ushort.MaxValue);
                    throw new SecurityException("Authenticator construction failed", ex);
                }
                rw.Write((ushort)0); //Selected index
                var auth = (Authenticator)authInstance;
                
                //Read challenge
                byte[] challenge = rw.ReadBytes();
                
                //Send response
                byte[] response = auth.ClientCreateResponse(_Connection.EndPoint, challenge);
                
                //Check result
                bool success = rw.ReadBool();
                
                if (success)
                    HandleAuthenticatedConnection();
                else throw new SecurityException("Authentication failed");
            }
        }
        
        private void HandleAuthenticatedConnection()
        {
            
        }
    }
}
