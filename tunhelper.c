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

extern int tun_alloc(char* dev, int flags, int* fd_or_errcode)
{
    int fd = open("/dev/net/tun", O_RDWR);
    if (fd < 0)
    {
        *fd_or_errcode = fd;
        return 1;
    }

    struct ifreq ifr;
    memset(&ifr, 0, sizeof(ifr));
    ifr.ifr_flags = flags;
    
    if (*dev)
        strncpy(ifr.ifr_name, dev, IFNAMSIZ);

    int err = ioctl(fd, TUNSETIFF, (void*)&ifr);
    if (err < 0)
    {
        close(fd);
        *fd_or_errcode = err;
        return 2;
    }

    *fd_or_errcode = fd;
    strcpy(dev, ifr.ifr_name);
    return 0;
}

extern int tun_set_persist(int fd, int persist)
{
    if (persist != 0)
        persist = 1;

    return ioctl(fd, TUNSETPERSIST, persist);
}

#ifdef __cplusplus
}
#endif
