using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace TA.SharpTunnel.Certificates
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
		
		public X509Chain GetChain()
		{
			X509Chain chain = new X509Chain();
			chain.Build(_Certificate);
			return chain;
		}
	}
}
