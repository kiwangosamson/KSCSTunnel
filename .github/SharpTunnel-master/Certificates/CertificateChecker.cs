using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace TA.UgconnectTunnel.Certificates
{
    public class CertificateChecker
    {
        private readonly X509Certificate2 _Certificate;
        private readonly FingerprintStore _FPStore;

        public CertificateChecker(X509Certificate2 Certificate) { _Certificate = Certificate; }

        public CertificateChecker(X509Certificate2 Certificate, FingerprintStore FingerprintStore)
            : this(Certificate)
        { _FPStore = FingerprintStore; }

        public bool CheckFingerprintStore()
        {
            if (_FPStore == null)
                throw new NullReferenceException("No FingerprintStore to work with");

            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                byte[] fingerprint = _Certificate.GetCertHash();
                return _FPStore.ContainsFingerprint(fingerprint);
            }
        }

        public bool CheckExpiration(DateTime? Date = null)
        {
            DateTime dt = Date.HasValue ? Date.Value : DateTime.Now;
            return dt < _Certificate.NotAfter && _Certificate.NotBefore < dt;
        }

        public X509Chain GetChain(X509RevocationMode RevocationMode, bool AllowUnknownCA)
        {
            X509Chain chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = RevocationMode;
            if (AllowUnknownCA)
                chain.ChainPolicy.VerificationFlags |= X509VerificationFlags.AllowUnknownCertificateAuthority;
            chain.Build(_Certificate);
            return chain;
        }

        private static X509KeyUsageFlags _GetUsage(X509Certificate2 cert)
        {
            foreach (X509Extension extension in cert.Extensions)
                if (extension is X509KeyUsageExtension)
                    return ((X509KeyUsageExtension)extension).KeyUsages;
            return X509KeyUsageFlags.None;
        }

        public X509KeyUsageFlags GetUsage() { return _GetUsage(_Certificate); }

        public static void PrintChain(X509Chain Chain)
        {
            for (int i = Chain.ChainElements.Count - 1; !(i < 0); i--)
            {
                X509ChainElement element = Chain.ChainElements[i];
                string name = DistinguishedNameParser.Parse(element.Certificate.SubjectName)["CN"];

                Console.Write(name);
                X509KeyUsageFlags flags = _GetUsage(element.Certificate);
                if (flags == X509KeyUsageFlags.None)
                    Console.Write(" [NoLimit]");
                else if (flags.HasFlag(X509KeyUsageFlags.KeyCertSign) && flags.HasFlag(X509KeyUsageFlags.CrlSign))
                    Console.Write(" [CrtSign]");
                if (element.Certificate.HasPrivateKey)
                    Console.Write(" [PriKey]");
                Console.WriteLine();

                foreach (X509ChainStatus status in element.ChainElementStatus)
                {
                    Console.WriteLine("`- " + status.Status);
                    Console.WriteLine("   " + status.StatusInformation);
                }
                if (i > 0)
                    Console.WriteLine("  |||");
            }
        }
    }
}
