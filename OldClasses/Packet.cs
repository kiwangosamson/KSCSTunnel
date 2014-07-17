/*

Auth protocol

S -> C      Available authenticators
ushort count; 
ushort[] authenticator_ids;

C -> S      Authenticator select
ushort authenticator_id;

S -> C      Challenge
uint length;
[ Authenticator specific ]

C -> S      Response
uint length;
[ Authenticator specific ]

S -> C      Success
byte success; // != 0 means yes

*/

using System;

namespace TA.SharpTunnel
{
    public static class Packet
    {
        public static byte[] CreateTunnelData(byte[] Data)
        {
            //TODO CreatTunnelData
            throw new NotImplementedException();
        }
        
        public static byte[] CreateAvailableAuthenticators(ushort[] AuthenticatorIDs)
        {
            byte[]packet = new byte[]
        }
    }
}
