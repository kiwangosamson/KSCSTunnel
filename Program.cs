using System;
using System.Text;
using TA.SharpTunnel.TunTap;

namespace TA.SharpTunnel
{
    public class Program
    {
        public static int Main(string[] args)
        {
            using (TunTapLinux dev = new TunTapLinux(TunTapDevice.DeviceType.TUN, false))
            {
                dev.Open();
                Console.WriteLine("Device opened: {0}", dev.Device);

                while (true)
                {
                    byte[] buff = new byte[1500];
                    int read = dev.Stream.Read(buff, 0, buff.Length);

                    var hexStringBuilder = new StringBuilder(read * 2);
                    for (int i = 0; i < read; i++)
                        hexStringBuilder.AppendFormat(" {0:x2}", buff[i]);

                    Console.WriteLine("{0} bytes -{1}", read, hexStringBuilder);
                }
            }

            //return 0;
        }
    }
}
