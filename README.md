C# VPN tunnel application
=========================


Building under Windows
----------------------

Bulding under Windows is NOT recommended, since Linux isn't supported.
Linux needs a correctly built tunhelper.so, and this file can only be
built under Linux.

If you still want to build the project under Windows:

- Create an empty file 'tunhelper.so' or copy tunhelper.so from Linux
- Build the project with Visual Studio 2012

Building under Linux
--------------------

Execute the following commands:

- ./tunhelper_build.sh
- xbuild
