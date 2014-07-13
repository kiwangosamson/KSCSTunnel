using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using Mono.Unix;
using Mono.Unix.Native;

public class TunTapLinux : TunTapDevice
{
 [DllImport("./tunhelper.so", CharSet = CharSet.Ansi, SetLastError = false)]
 private static extern int tun_alloc(StringBuilder dev, int flags);

 public TunTapLinux(DeviceType Type, bool Persist)
 {
  //Check if Linux
  //Check for tun module
  //Check if tunhelper.so exists
 }

 public override void Open()
 {
  
 }

 public override void Close()
 {
  
 } 

 public static int tun_alloc(ref string dev, int flags)
 {
  if (dev.Length > 15)
   throw new ArgumentException("dev too long (>15)");

  StringBuilder sb = new StringBuilder(17);
  int return_value = _tun_alloc(sb, flags);
  if (return_value < 0)
   throw new Exception("Native error #" + return_value);

  dev = sb.ToString();
  return return_value;
 }

 public static void Main()
 {
  ushort IFF_TUN = 1;
  ushort IFF_TAP = 2;
  ushort IFF_NO_PI = 0x1000;
  ushort IFF_PERSIST = 0x0800;

  string dev_name = string.Empty;
  int fd = tun_alloc(ref dev_name, IFF_TUN | IFF_NO_PI | IFF_PERSIST);

  Console.WriteLine("Device: /dev/{0} FileDescriptor: {1}", dev_name, fd);

  using (UnixStream fs = new UnixStream(fd))
  {
   while (true)
   {
    byte[] buffer = new byte[1500];
    int read = fs.Read(buffer, 0, buffer.Length);
    Console.WriteLine("Read {0} bytes", read);
   }
  }
 }
}
