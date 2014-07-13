#include <fcntl.h>
#include <string.h>
#include <sys/socket.h>
#include <linux/ioctl.h>
#include <linux/if.h>
#include <linux/if_tun.h>

#ifdef __cplusplus

using namespace std;

extern "C"
{

#endif

extern int tun_alloc(char* dev, int flags)
{
 int fd = open("/dev/net/tun", O_RDWR);

 if (fd < 0)
 {
  perror("Opening /dev/net/tun");
  return fd;
 }

 struct ifreq ifr;
 memset(&ifr, 0, sizeof(ifr));

 ifr.ifr_flags = flags;
 if (*dev)
  strncpy(ifr.ifr_name, dev, IFNAMSIZ);

 int err = ioctl(fd, TUNSETIFF, (void*)&ifr);

 if (err < 0)
 {
  perror("ioctl(TUNSETIFF)");
  close(fd);
  return err;
 }

 strcpy(dev, ifr.ifr_name);
 return fd;
}

#ifdef __cplusplus

}

#endif
