At work, i had to find a way to create new TCP/IP printer ports on a remote print server from the .NET code of my application. I couldn't use WMI, so i had to find something else. Luckily, the <a href="http://msdn2.microsoft.com/en-us/library/aa506528.aspx">XcvData</a> Windows function does just that. Unfortunately, it is a royal pain in the ass to use and there's not a lot of documentation available on how to use it.  And i certainly didn't find anything on how to call it from .NET code. So after wasting about 2 days on trying to get this to work, i figured i might as well put the solution online in case anyone needs to do this from .NET code again.

This is the signature of the function:

<script src="https://gist.github.com/3611711.js?file=s1.c"></script>

Just looking at that makes me feel bad for everyone who's ever had to code against Windows API's. Anyway, according to the documentation, the first parameter (hXcv) should be a handle to the print server (which you can retrieve with a call to OpenPrinter), the second parameter (pszDataName) has to be "AddPort" if you want the function to create a new port.  And then comes the fun part... the third parameter (pInputData) should be a pointer to a <a href="http://msdn2.microsoft.com/en-us/library/aa506565.aspx">PORT_DATA_1</a> structure and the fourth parameter has to contain the size in bytes of the PORT_DATA_1 structure you passed as the third argument.  The other parameters can be ignored (nice API design btw) except for the last one, which is an out parameter that will return a numeric code which will indicate either success or the cause of the failure.

I had a lot of problems trying to pass a pointer to a valid PORT_DATA_1 structure.  The structure looks like this:

<script src="https://gist.github.com/3611711.js?file=s2.c"></script>

As you can see, the struct contains a couple of Unicode character arrays and even a byte array.  Defining a struct in C# that could be marshalled to this turned out to be the tricky part in getting this stuff to work.

But first of all, we needed to be able to call the OpenPrinter function to retrieve a handle to the print server where we need to create the new printer port:

<script src="https://gist.github.com/3611711.js?file=s3.cs"></script>

Allright, now can retrieve the handle with a call to OpenPrinter and we can cleanup afterwards by passing the handle to the ClosePrinter function.

Now we need a C# definition of a struct that can be marshalled to a PORT_DATA_1 struct:

<script src="https://gist.github.com/3611711.js?file=s4.cs"></script>

First of all, the struct has to have Sequential as its LayoutKind, and each string must be marshalled as a unicode string (.NET strings are unicode by default, but when marshalled to native code they are converted to ANSI strings, so the CharSet setting is definitely required). Then, for each array in the original struct, you need to make sure our string is converted to an array of the expected size. Marshalling those strings as ByValTStr and setting the SizeConst parameter did the trick there.  Then there's the byte array in the original struct.  The function expects there to be a byte array of 540 elements. Marshalling it as ByValArray and setting the SizeConst makes that work as well.

Right, now we have the structure, so we still need a way to call the XcvData function:

<script src="https://gist.github.com/3611711.js?file=s5.cs"></script>

Notice how the DllImport attribute has its CharSet parameter set to unicode as well.  If you don't do this, the function call will crash your app (can't even catch an exception) because it expects pszDataName to be a unicode string and as mentioned earlier, without specifying CharSet.Unicode it would've been marshalled to an ANSI string. Happy times.

Anyways, creating a TCP/IP printer port on a remote server is now as simple as this:

<script src="https://gist.github.com/3611711.js?file=s6.cs"></script>

I don't wanna look at any Windows functions for at least a couple of months :)
