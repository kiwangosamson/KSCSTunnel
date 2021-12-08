using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using Mono.Unix;

namespace TA.UgconnectTunnel.TunTap
{
    public class TunTapLinux : TunTapDevice
    {
        private const byte IFNAMSIZ = 16;
        private const ushort IFF_TUN = 1;
        private const ushort IFF_TAP = 2;
        private const ushort IFF_NO_PI = 0x1000;

        //Returns 0 on success, other value on error
        //dev can be null, will be set to the actual if name
        //fd_or_errcode contains the fd on success, errcode on failure
        [DllImport("./tunhelper.so", CharSet = CharSet.Ansi, SetLastError = false)]
        private static extern int tun_alloc(StringBuilder dev, int flags, out int fd_or_errcode);

        [DllImport("./tunhelper.so", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int tun_set_persist(int fd, bool persist);

        private DeviceType _Type;
        private UnixStream _Stream;
        private bool _Persist = false;
        private string _Interface = null;
        private bool _Opened = false;

        public override string Interface { get { return _Interface; } }
        public override TunTapDevice.DeviceType Type { get { return _Type; } }
        public override Stream Stream { get { return _Stream; } }
        public bool Persist { get { return _Persist; } }

        public TunTapLinux(DeviceType Type, bool Persist = false, string Interface = null)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                throw new PlatformNotSupportedException("Only Linux is supported");

            if (!File.Exists("/dev/net/tun"))
                throw new FileNotFoundException("/dev/net/tun not found");

            if (!File.Exists("tunhelper.so"))
                throw new FileNotFoundException("tunhelper.so not found");

            if (!string.IsNullOrWhiteSpace(Interface))
            {
                _Interface = Interface.Trim();
                if (_Interface.Length > 15) //15 or 16 limit? 15 because of CString \0 at the end?
                    throw new ArgumentException("Device name too long (>15)");
            }
            else _Interface = null;

            _Type = Type;
            _Persist = Persist;
        }

        public override void Open()
        {
            if (_Opened)
                throw new InvalidOperationException("Already opened");

            StringBuilder dev_sb = new StringBuilder(IFNAMSIZ);
            if (_Interface != null)
                dev_sb.Append(_Interface);

            int flags = 0;
            if (_Type == DeviceType.TUN)
                flags |= IFF_TUN;
            else if (_Type == DeviceType.TAP)
                flags |= IFF_TAP;
            else throw new Exception("Unknwo device type");

            int fd_or_errcode;
            int errno = tun_alloc(dev_sb, flags, out fd_or_errcode);

            if (errno != 0)
                switch (errno)
                {
                    case 1: throw new UnixIOException("Can't open /dev/net/tun (" + fd_or_errcode + ")");
                    case 2: throw new UnixIOException("ioctl(TUNSETIFF) failed (" + fd_or_errcode + ")");
                    default: throw new Exception("Unknown error (" + fd_or_errcode + ")");
                }

            int err = tun_set_persist(fd_or_errcode, _Persist);
            if (err != 0)
                throw new UnixIOException("ioctl(TUNSETPERSIST) failed (" + err + ")");

            _Interface = dev_sb.ToString();
            _Stream = new UnixStream(fd_or_errcode);
            _Opened = true;
        }

        public override void Close()
        {
            if (!_Opened)
                throw new InvalidOperationException("Not opened");

            _Stream.Dispose();
            _Stream = null;

            _Opened = false;
        }
    }
}
