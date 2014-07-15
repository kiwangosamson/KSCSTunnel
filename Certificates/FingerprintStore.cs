using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace TA.SharpTunnel.Certificates
{
	public class FingerprintStore
	{
		private readonly string _Path;
		private readonly List<byte[]> _Fingerprints = new List<byte[]>();
		
		public string Path { get { return _Path; } }
		
		public FingerprintStore(string Path)
		{
			_Path = Path;
			Reload();
		}
		
		public void Reload()
		{
			_Fingerprints.Clear();
			if (File.Exists(_Path))
			{
				using (FileStream fs = new FileStream(_Path, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (StreamReader reader = new StreamReader(fs, Encoding.ASCII))
				{
					byte[] fp = Convert.FromBase64String(reader.ReadLine());
					_Fingerprints.Add(fp);
				}
			}
		}
		
		public void Save()
		{
			using (FileStream fs = new FileStream(_Path, FileMode.Create, FileAccess.Write, FileShare.None))
			using (StreamWriter writer = new StreamWriter(fs, Encoding.ASCII))
				foreach (byte[] fp in _Fingerprints)
				{
					string fp_string = Convert.ToBase64String(fp);
					writer.WriteLine(fp_string);
				}
		}
		
		private bool CompareArrays(byte[] a1, byte[] a2)
		{
			if (a1.Length == a2.Length)
			{
				for (int i = 0; i < a1.Length; i++)
					if (a1[i] != a2[i])
						return false;
				return true;
			}
			else return false;
		}
		
		public bool ContainsFingerprint(byte[] Fingerprint)
		{
			for (int i = 0; i < _Fingerprints.Count; i++)
				if (CompareArrays(_Fingerprints[i], Fingerprint))
					return true;
			return false;
		}
		
		public void AddFingerprint(byte[] Fingerprint)
		{
			if (!ContainsFingerprint(Fingerprint))
				_Fingerprints.Add(Fingerprint);
		}
		
		public void RemoveFingerprint(byte[] Fingerprint) 
		{
			for (int i = _Fingerprints.Count - 1; !(i < 0); i--)
				if (CompareArrays(_Fingerprints[i], Fingerprint))
					_Fingerprints.RemoveAt(i);
		}
	}
}
