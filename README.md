C# VPN tunnel application
=========================


Building under Windows
----------------------

Bulding under Windows is NOT recommended, since Linux isn't supported.
Linux needs a correctly built tunhelper.so, and this file can only
be created under Linux.

If you still wan't to build the project under Windows:

- Create an empty file 'tunhelper.so' or copy tunhelper.so from Linux
- Build project with Visual Studio 2012

Building under Linux
--------------------

- Execute tunhelper_build.so
- Execute 'xbuild'
