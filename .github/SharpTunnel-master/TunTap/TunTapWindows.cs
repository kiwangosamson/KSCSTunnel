using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace TA.UgconnectTunnel.TunTap
{
    public class TunTapWindows : TunTapDevice
    {
        private const uint METHOD_BUFFERED = 0;
        private const uint FILE_ANY_ACCESS = 0;
        private const uint FILE_DEVICE_UNKNOWN = 0x00000022;
        private const int FILE_ATTRIBUTE_SYSTEM = 0x4;
        private const int FILE_FLAG_OVERLAPPED = 0x40000000;
        private readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(string filename, [MarshalAs(UnmanagedType.U4)]FileAccess fileaccess, [MarshalAs(UnmanagedType.U4)]FileShare fileshare, int securityattributes, [MarshalAs(UnmanagedType.U4)]FileMode creationdisposition, int flags, IntPtr template);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out int lpBytesReturned, IntPtr lpOverlapped);

        public static Dictionary<string, Guid> Interfaces
        {
            get
            {
                var _Guids = new List<Guid>();
                using (RegistryKey regAdapters = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4D36E972-E325-11CE-BFC1-08002BE10318}", false))
                    foreach (string subKey in regAdapters.GetSubKeyNames())
                        try
                        {
                            using (RegistryKey regAdapter = regAdapters.OpenSubKey(subKey))
                            {
                                string componentId = regAdapter.GetValue("ComponentId").ToString();
                                if (componentId != null)
                                    if ((componentId == "tap0801") || (componentId == "tap0901"))
                                    {
                                        string sGuid = regAdapter.GetValue("NetCfgInstanceId").ToString();
                                        _Guids.Add(new Guid(sGuid));
                                    }
                            }
                        }
                        catch { }

                var _Adapters = new Dictionary<string, Guid>(_Guids.Count);
                foreach (Guid guid in _Guids)
                    using (RegistryKey regConnection = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\" + guid.ToString("B") + "\\Connection", false))
                    {
                        string name = regConnection.GetValue("Name").ToString();
                        if (name != null)
                            _Adapters.Add(name, guid);
                        else _Adapters.Add(string.Empty, guid);
                    }

                return _Adapters;
            }
        }

        private DeviceType _Type;
        private SafeFileHandle _Handle;
        private FileStream _Stream;
        private bool _Opened = false;
        private string _Interface = null;
        private string _DevicePath = null;

        public override string Interface { get { return _Interface; } }
        public override TunTapDevice.DeviceType Type { get { return _Type; } }
        public override Stream Stream { get { return _Stream; } }

        public TunTapWindows(DeviceType Type)
        {
            var interfaces = Interfaces;
            if (interfaces.Count > 0)
                foreach (var intf in interfaces)
                {
                    Init(Type, intf.Value, intf.Key);
                    break;
                }
            else throw new FileNotFoundException("No interfaces found");
        }

        public TunTapWindows(DeviceType Type, string Interface) { Init(Type, Interfaces[Interface], Interface); }

        public TunTapWindows(DeviceType Type, Guid Guid)
        {
            var interfaces = Interfaces;
            foreach (var intf in interfaces)
                if (intf.Value == Guid)
                {
                    Init(Type, Guid, intf.Key);
                    return;
                }
            throw new FileNotFoundException("Interface not found");
        }

        private void Init(DeviceType type, Guid guid, string name)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new PlatformNotSupportedException("Only Windows is supported");

            _Type = type;
            _Interface = name;
            _DevicePath = @"\\.\Global\" + guid.ToString("B") + ".tap";
        }

        public override void Open()
        {
            if (_Opened)
                throw new InvalidOperationException("Already opened");

            int len = 0;
            IntPtr ptr = CreateFile(_DevicePath, FileAccess.ReadWrite, FileShare.ReadWrite, 0, FileMode.Open, FILE_ATTRIBUTE_SYSTEM | FILE_FLAG_OVERLAPPED, IntPtr.Zero);

            IntPtr pstatus = Marshal.AllocHGlobal(4);
            try
            {
                Marshal.WriteInt32(pstatus, 1);
                if (!DeviceIoControl(ptr, TAP_CONTROL_CODE(6, METHOD_BUFFERED), pstatus, 4, pstatus, 4, out len, IntPtr.Zero)) //TAP_IOCTL_SET_MEDIA_STATUS 
                    throw new Win32Exception();
            }
            finally { Marshal.FreeHGlobal(pstatus); }

            IntPtr ptun = Marshal.AllocHGlobal(12);
            try
            {
                Marshal.WriteInt32(ptun, 0, 0x0100030a);
                Marshal.WriteInt32(ptun, 4, 0x0000030a);
                Marshal.WriteInt32(ptun, 8, unchecked((int)0x00ffffff));
                if (!DeviceIoControl(ptr, TAP_CONTROL_CODE(10, METHOD_BUFFERED), ptun, 12, ptun, 12, out len, IntPtr.Zero)) //TAP_IOCTL_CONFIG_TUN
                    throw new Win32Exception();
            }
            finally { Marshal.FreeHGlobal(ptun); }

            _Handle = new SafeFileHandle(ptr, true);
            _Stream = new FileStream(_Handle, FileAccess.ReadWrite, 4096, true);
            _Opened = true;
        }

        public override void Close()
        {
            if (!_Opened)
                throw new InvalidOperationException("Not opened");

            _Stream.Dispose();
            _Stream = null;

            _Handle.Dispose();

            _Opened = false;
        }

        private static uint TAP_CONTROL_CODE(uint request, uint method) { return ((FILE_DEVICE_UNKNOWN << 16) | (FILE_ANY_ACCESS << 14) | (request << 2) | method); }
    }
}
