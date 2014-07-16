using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace TA.SharpTunnel.Connections
{
	public class ClientConnection : Connection
	{
		private IPEndPoint _EndPoint = null;
		private TcpClient _Client = new TcpClient();
		private SslStream _Stream = null;
		private bool _Connected = false;
		
		public override Stream Stream { get { return _Stream; } }
		public override IPEndPoint EndPoint { get { return _EndPoint; } }
		
		public ClientConnection(IPEndPoint EndPoint) { _EndPoint = EndPoint; }
		
		public override void Connect()
		{
			if (_Connected)
				throw new InvalidOperationException("Already connected");
			
			_Client.Connect(_EndPoint);
			
			NetworkStream ns = _Client.GetStream();
			_Stream = new SslStream(ns, false, CheckCertificate);
			
			_Stream.AuthenticateAsClient("SharpTunnel");
			
			_Connected = true;
		}
		
		private bool CheckCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			//TODO Implement certificate check
			return true;
		}
		
		public override void Disconnect()
		{
			if (!_Connected)
				throw new InvalidOperationException("Not connected");
			
			_Stream.Dispose();
			_Stream = null;
			
			_Client.Close();
			_Client = null;
		}
	}
}
