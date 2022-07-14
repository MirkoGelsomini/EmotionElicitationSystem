# Bitalino dll

Source: https://github.com/BITalinoWorld/dotNet-api


## Compiling the class library

You can compile the class library if you don't want to use the pre-built assemblies or if you need to build an assembly for Windows XP or for a different version of the .NET Framework. You will need [bitalino.cpp](../../../cpp-api/tree/master/bitalino.cpp) and [bitalino.h](../../../cpp-api/tree/master/bitalino.h) from the [C++ API](../../../cpp-api).

To compile the class library:
- create a CLR Class Library project in Visual Studio;
- remove from project all files added automatically to the project;
- disable Precompiled Headers in Project Properties → Configuration Properties → C/C++ → Precompiled Headers → Precompiled Header: Not Using Precompiled Headers
- copy [bitalino.cpp](../../../cpp-api/tree/master/bitalino.cpp), [bitalino.h](../../../cpp-api/tree/master/bitalino.h) and [dotNet_wrapper.cpp](dotNet_wrapper.cpp) to the project directory;
- add bitalino.cpp and dotNet_wrapper.cpp files to the project at the “Source Files” folder;
- add a reference to `ws2_32.lib` in Project Properties → Configuration Properties → Linker → Input → Additional Dependencies;
- build the solution.

