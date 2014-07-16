using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace TA.SharpTunnel.Certificates
{
    public static class DistinguishedNameParser
    {
        public static Dictionary<string, string> Parse(X500DistinguishedName DistinguishedName)
        {
            string[] parts = DistinguishedName.Decode(X500DistinguishedNameFlags.UseNewLines).Split(new char[2] { '\r', '\n' });
            var dict = new Dictionary<string, string>(parts.Length);
            foreach (string str in parts)
                if (!string.IsNullOrWhiteSpace(str))
                {
                    int indexOfEq = str.IndexOf('=');
                    string key = str.Substring(0, indexOfEq);
                    string value = str.Substring(indexOfEq + 1);
                    dict.Add(key, value);
                }
            return dict;
        }
    }
}
