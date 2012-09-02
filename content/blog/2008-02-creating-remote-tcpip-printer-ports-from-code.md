At work, i had to find a way to create new TCP/IP printer ports on a remote print server from the .NET code of my application. I couldn't use WMI, so i had to find something else. Luckily, the <a href="http://msdn2.microsoft.com/en-us/library/aa506528.aspx">XcvData</a> Windows function does just that. Unfortunately, it is a royal pain in the ass to use and there's not a lot of documentation available on how to use it.  And i certainly didn't find anything on how to call it from .NET code. So after wasting about 2 days on trying to get this to work, i figured i might as well put the solution online in case anyone needs to do this from .NET code again.

This is the signature of the function:

<pre>
BOOL WINAPI XcvData(HANDLE  hXcv, LPCWSTR  pszDataName, PBYTE  pInputData, DWORD  cbInputData,
    PBYTE  pOutputData, DWORD  cbOutputData, PDWORD  pcbOutputNeeded, PDWORD  pdwStatus);
</pre>

Just looking at that makes me feel bad for everyone who's ever had to code against Windows API's. Anyway, according to the documentation, the first parameter (hXcv) should be a handle to the print server (which you can retrieve with a call to OpenPrinter), the second parameter (pszDataName) has to be "AddPort" if you want the function to create a new port.  And then comes the fun part... the third parameter (pInputData) should be a pointer to a <a href="http://msdn2.microsoft.com/en-us/library/aa506565.aspx">PORT_DATA_1</a> structure and the fourth parameter has to contain the size in bytes of the PORT_DATA_1 structure you passed as the third argument.  The other parameters can be ignored (nice API design btw) except for the last one, which is an out parameter that will return a numeric code which will indicate either success or the cause of the failure.

I had a lot of problems trying to pass a pointer to a valid PORT_DATA_1 structure.  The structure looks like this:

<pre>
typedef struct _PORT_DATA_1 {
    WCHAR  sztPortName[MAX_PORTNAME_LEN];
    DWORD  dwVersion;
    DWORD  dwProtocol;
    DWORD  cbSize;
    DWORD  dwReserved;
    WCHAR  sztHostAddress[MAX_NETWORKNAME_LEN];
    WCHAR  sztSNMPCommunity[MAX_SNMP_COMMUNITY_STR_LEN];
    DWORD  dwDoubleSpool;
    WCHAR  sztQueue[MAX_QUEUENAME_LEN];
    WCHAR  sztIPAddress[MAX_IPADDR_STR_LEN];
    BYTE   Reserved[540];
    DWORD  dwPortNumber;
    DWORD  dwSNMPEnabled;
    DWORD  dwSNMPDevIndex;
} PORT_DATA_1, *PPORT_DATA_1;
</pre>

As you can see, the struct contains a couple of Unicode character arrays and even a byte array.  Defining a struct in C# that could be marshalled to this turned out to be the tricky part in getting this stuff to work.

But first of all, we needed to be able to call the OpenPrinter function to retrieve a handle to the print server where we need to create the new printer port:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">enum</span> <span style="color:#2b91af;">PrinterAccess</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; ServerAdmin = 0x01,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; ServerEnum = 0x02,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; PrinterAdmin = 0x04,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; PrinterUse = 0x08,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; JobAdmin = 0x10,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; JobRead = 0x20,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; StandardRightsRequired = 0x000f0000,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; PrinterAllAccess = (StandardRightsRequired | PrinterAdmin | PrinterUse)</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">StructLayout</span>(<span style="color:#2b91af;">LayoutKind</span>.Sequential)]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">struct</span> <span style="color:#2b91af;">PrinterDefaults</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">IntPtr</span> pDataType;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">IntPtr</span> pDevMode;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">PrinterAccess</span> DesiredAccess;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">DllImport</span>(<span style="color:#a31515;">"winspool.drv"</span>, SetLastError = <span style="color:blue;">true</span>)]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">static</span> <span style="color:blue;">extern</span> <span style="color:blue;">int</span> OpenPrinter(<span style="color:blue;">string</span> printerName, <span style="color:blue;">out</span> <span style="color:#2b91af;">IntPtr</span> phPrinter,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">ref</span> <span style="color:#2b91af;">PrinterDefaults</span> printerDefaults);</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">DllImport</span>(<span style="color:#a31515;">"winspool.drv"</span>, SetLastError = <span style="color:blue;">true</span>)]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">static</span> <span style="color:blue;">extern</span> <span style="color:blue;">int</span> ClosePrinter(<span style="color:#2b91af;">IntPtr</span> phPrinter);</p>
</div>

Allright, now can retrieve the handle with a call to OpenPrinter and we can cleanup afterwards by passing the handle to the ClosePrinter function.

Now we need a C# definition of a struct that can be marshalled to a PORT_DATA_1 struct:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">const</span> <span style="color:blue;">int</span> MAX_PORTNAME_LEN = 64;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">const</span> <span style="color:blue;">int</span> MAX_NETWORKNAME_LEN = 49;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">const</span> <span style="color:blue;">int</span> MAX_SNMP_COMMUNITY_STR_LEN = 33;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">const</span> <span style="color:blue;">int</span> MAX_QUEUENAME_LEN = 33;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">const</span> <span style="color:blue;">int</span> MAX_IPADDR_STR_LEN = 16;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">const</span> <span style="color:blue;">int</span> RESERVED_BYTE_ARRAY_SIZE = 540;</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">StructLayout</span>(<span style="color:#2b91af;">LayoutKind</span>.Sequential, CharSet = <span style="color:#2b91af;">CharSet</span>.Unicode)]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">struct</span> <span style="color:#2b91af;">PortData</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">MarshalAs</span>(<span style="color:#2b91af;">UnmanagedType</span>.ByValTStr, SizeConst = MAX_PORTNAME_LEN)]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">string</span> sztPortName;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">UInt32</span> dwVersion;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">UInt32</span> dwProtocol;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">UInt32</span> cbSize;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">UInt32</span> dwReserved;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">MarshalAs</span>(<span style="color:#2b91af;">UnmanagedType</span>.ByValTStr, SizeConst = MAX_NETWORKNAME_LEN)]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">string</span> sztHostAddress;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">MarshalAs</span>(<span style="color:#2b91af;">UnmanagedType</span>.ByValTStr, SizeConst = MAX_SNMP_COMMUNITY_STR_LEN)]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">string</span> sztSNMPCommunity;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">UInt32</span> dwDoubleSpool;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">MarshalAs</span>(<span style="color:#2b91af;">UnmanagedType</span>.ByValTStr, SizeConst = MAX_QUEUENAME_LEN)]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">string</span> sztQueue;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">MarshalAs</span>(<span style="color:#2b91af;">UnmanagedType</span>.ByValTStr, SizeConst = MAX_IPADDR_STR_LEN)]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">string</span> sztIPAddress;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">MarshalAs</span>(<span style="color:#2b91af;">UnmanagedType</span>.ByValArray, SizeConst = RESERVED_BYTE_ARRAY_SIZE)]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">byte</span>[] Reserved;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">UInt32</span> dwPortNumber;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">UInt32</span> dwSNMPEnabled;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:#2b91af;">UInt32</span> dwSNMPDevIndex;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

First of all, the struct has to have Sequential as its LayoutKind, and each string must be marshalled as a unicode string (.NET strings are unicode by default, but when marshalled to native code they are converted to ANSI strings, so the CharSet setting is definitely required). Then, for each array in the original struct, you need to make sure our string is converted to an array of the expected size. Marshalling those strings as ByValTStr and setting the SizeConst parameter did the trick there.  Then there's the byte array in the original struct.  The function expects there to be a byte array of 540 elements. Marshalling it as ByValArray and setting the SizeConst makes that work as well.

Right, now we have the structure, so we still need a way to call the XcvData function:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; [<span style="color:#2b91af;">DllImport</span>(<span style="color:#a31515;">"winspool.drv"</span>, SetLastError = <span style="color:blue;">true</span>, CharSet = <span style="color:#2b91af;">CharSet</span>.Unicode)]</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">public</span> <span style="color:blue;">static</span> <span style="color:blue;">extern</span> <span style="color:blue;">int</span> XcvDataW(<span style="color:#2b91af;">IntPtr</span> hXcv, <span style="color:blue;">string</span> pszDataName,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">IntPtr</span> pInputData, <span style="color:#2b91af;">UInt32</span> cbInputData, <span style="color:blue;">out</span> <span style="color:#2b91af;">IntPtr</span> pOutputData, <span style="color:#2b91af;">UInt32</span> cbOutputData,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">out</span> <span style="color:#2b91af;">UInt32</span> pcbOutputNeeded, <span style="color:blue;">out</span> <span style="color:#2b91af;">UInt32</span> pdwStatus);</p>
</div>

Notice how the DllImport attribute has its CharSet parameter set to unicode as well.  If you don't do this, the function call will crash your app (can't even catch an exception) because it expects pszDataName to be a unicode string and as mentioned earlier, without specifying CharSet.Unicode it would've been marshalled to an ANSI string. Happy times.

Anyways, creating a TCP/IP printer port on a remote server is now as simple as this:

<div style="font-family:Consolas;font-size:10pt;color:black;background:white;">
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">IntPtr</span> printerHandle;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">InteropStuff</span>.<span style="color:#2b91af;">PrinterDefaults</span> defaults = </p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">new</span> <span style="color:#2b91af;">InteropStuff</span>.<span style="color:#2b91af;">PrinterDefaults</span> { DesiredAccess = <span style="color:#2b91af;">InteropStuff</span>.<span style="color:#2b91af;">PrinterAccess</span>.ServerAdmin };</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">InteropStuff</span>.OpenPrinter(<span style="color:#a31515;">@"\myPrintServer,XcvMonitor Standard TCP/IP Port"</span>, <span style="color:blue;">out</span> printerHandle, <span style="color:blue;">ref</span> defaults);</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">InteropStuff</span>.<span style="color:#2b91af;">PortData</span> portData = <span style="color:blue;">new</span> <span style="color:#2b91af;">InteropStuff</span>.<span style="color:#2b91af;">PortData</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; dwVersion = 1, <span style="color:green;">// has to be 1 for some unknown reason</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; dwProtocol = 1, <span style="color:green;">// 1 = RAW, 2 = LPR</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; dwPortNumber = 9100, <span style="color:green;">// 9100 = default port for RAW, 515 for LPR</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; dwReserved = 0, <span style="color:green;">// has to be 0 for some unknown reason</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; sztPortName = <span style="color:#a31515;">"DBR_172.30.164.15"</span>,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; sztIPAddress = <span style="color:#a31515;">"172.30.164.15"</span>,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; sztSNMPCommunity = <span style="color:#a31515;">"public"</span>,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; dwSNMPEnabled = 1,</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; dwSNMPDevIndex = 1</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; };</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">uint</span> size = (<span style="color:blue;">uint</span>)<span style="color:#2b91af;">Marshal</span>.SizeOf(portData);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; portData.cbSize = size;</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">IntPtr</span> pointer = <span style="color:#2b91af;">Marshal</span>.AllocHGlobal((<span style="color:blue;">int</span>)size);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">Marshal</span>.StructureToPtr(portData, pointer, <span style="color:blue;">true</span>);</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">try</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">IntPtr</span> outputData;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">UInt32</span> outputNeeded;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">UInt32</span> status;</p>
<p style="margin:0;">&nbsp;</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">InteropStuff</span>.XcvDataW(printerHandle, <span style="color:#a31515;">"AddPort"</span>, pointer, size, <span style="color:blue;">out</span> outputData, 0, </p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">out</span> outputNeeded, <span style="color:blue;">out</span> status);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:blue;">finally</span></p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; {</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">InteropStuff</span>.ClosePrinter(printerHandle);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; <span style="color:#2b91af;">Marshal</span>.FreeHGlobal(pointer);</p>
<p style="margin:0;">&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp; }</p>
</div>

I don't wanna look at any Windows functions for at least a couple of months :)
