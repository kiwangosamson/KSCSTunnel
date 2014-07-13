using System;
using System.IO;

namespace TA.SharpTunnel.TunTap
{
    public abstract class TunTapDevice : IDisposable
    {
        public enum DeviceType : int
        {
            TUN = 1,
            TAP = 2
        }

        public abstract DeviceType Type { get; }
        public abstract Stream Stream { get; }

        public abstract void Open();
        public abstract void Close();

        public virtual void Dispose() { Close(); }
    }
}
