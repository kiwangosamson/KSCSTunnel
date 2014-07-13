using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using Mono.Unix;

namespace TA.SharpTunnel.TunTap
{
    public class TunTapLinux : TunTapDevice
    {
        private const byte IFNAMSIZ = 16;
        private const ushort IFF_TUN = 1;
        private const ushort IFF_TAP = 2;
        private const ushort IFF_NO_PI = 0x1000;
        private const ushort IFF_PERSIST = 0x0800;

        //Returns 0 on success, other value on error
        //FD contains the tun/tap file descriptor
        //errcode contains the native error code if failed
        //dev contains the device name, e.g. tun0
        [DllImport("./tunhelper.so", CharSet = CharSet.Ansi, SetLastError = false)]
        private static extern int tun_alloc(StringBuilder dev, int flags, out int fd_or_errcode);

        private DeviceType _Type;
        private UnixStream _Stream;
        private bool _Persist = false;
        private string _Device = null;
        private bool _Opened = false;

        public override TunTapDevice.DeviceType Type { get { return _Type; } }
        public override Stream Stream { get { return _Stream; } }
        public bool Persist { get { return _Persist; } }
        public string Device { get { return _Device; } }

        public TunTapLinux(DeviceType Type, bool Persist, string Device = null)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                throw new PlatformNotSupportedException("Only linux is supported");

            if (!File.Exists("/dev/net/tun"))
                throw new FileNotFoundException("/dev/net/tun not found");

            if (!File.Exists("tunhelper.so"))
                throw new FileNotFoundException("tunhelper.so not found");

            if (!string.IsNullOrWhiteSpace(Device))
            {
                _Device = Device.Trim();
                if (_Device.Length > 15)
                    throw new ArgumentException("Device name too long (>15)");
            }
            else _Device = null;

            _Type = Type;
            _Persist = Persist;
        }

        public override void Open()
        {
            if (_Opened)
                throw new InvalidOperationException("Already opened");

            StringBuilder dev_sb = new StringBuilder(IFNAMSIZ);
            if (_Device != null)
                dev_sb.Append(_Device);

            int flags = 0;
            if (_Type == DeviceType.TUN)
                flags |= IFF_TUN;
            else if (_Type == DeviceType.TAP)
                flags |= IFF_TAP;
            else throw new Exception("Unknwo device type");
            if (_Persist)
                flags |= IFF_PERSIST;

            int fd_or_errcode;
            int errno = tun_alloc(dev_sb, flags, out fd_or_errcode);

            switch (errno)
            {
                case 0:
                    _Device = dev_sb.ToString();
                    _Stream = new UnixStream(fd_or_errcode);
                    _Opened = true;
                    return;

                case 1: throw new UnixIOException("Can't open /dev/net/tun (" + fd_or_errcode + ")");
                case 2: throw new UnixIOException("ioctl(TUNSETIFF) failed (" + fd_or_errcode + ")");
                default: throw new Exception("Unknown error (" + fd_or_errcode + ")");
            }
        }

        public override void Close()
        {
            if (!_Opened)
                throw new InvalidOperationException("Not opened");

            _Stream.Close();
            _Stream = null;

            _Opened = false;
        }
    }
}
