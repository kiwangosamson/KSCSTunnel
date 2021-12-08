using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace TA.UgconnectTunnel.Connection
{
	public class ServerConnection : NetConnection
	{
		private X509Certificate _Certificate = null;
		private IPEndPoint _EndPoint = null;
		private TcpClient _Client = new TcpClient();
		private SslStream _Stream = null;
		private bool _Connected = false;
		private TcpListener _Listener = null;
		private bool _Started = false;
		
		public override Stream Stream { get { return _Stream; } }
		public override IPEndPoint EndPoint { get { return _EndPoint; } }
		
		public ServerConnection(IPEndPoint LocalEndPoint, X509Certificate Certificate)
		{
			_EndPoint = LocalEndPoint;
			_Listener = new TcpListener(_EndPoint);
			_Certificate = Certificate;
		}
		
		public void Start()
		{
			if (_Started)
				throw new InvalidOperationException("Already started");
			
			_Listener.Start();
			_Started = true;
		}
		
		public override void Connect()
		{
			if (!_Started)
				throw new NotImplementedException("Listener not running");
			
			if (_Connected)
				throw new InvalidOperationException("Already connected");
			
			_Client = _Listener.AcceptTcpClient();
			
			NetworkStream ns = _Client.GetStream();
			_Stream = new SslStream(ns, false, (object sndr, X509Certificate cert, X509Chain chain, SslPolicyErrors err) => true);
			
			_Stream.AuthenticateAsServer(_Certificate, false, SslProtocols.Tls12, false);
			
			_Connected = true;
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
		
		public void Stop()
		{
			if (!_Started)
				throw new InvalidOperationException("Not started");
			
			_Listener.Stop();
			_Started = false;
		}
		
		public override void Dispose()
		{
			base.Dispose();
			try { Stop(); }
			catch { }
		}
	}
}
