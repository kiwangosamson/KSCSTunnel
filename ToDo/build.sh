gcc -shared -o tunhelper.so tunhelper.c -fPIC -O2
mcs -reference:Mono.Posix.dll main.cs

