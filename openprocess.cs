using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    // OpenProcess function from kernel32.dll
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(
        ProcessAccessFlags dwDesiredAccess,
        bool bInheritHandle,
        int dwProcessId
    );

    // VirtualQueryEx function from kernel32.dll
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int VirtualQueryEx(
        IntPtr hProcess,
        IntPtr lpAddress,
        out MEMORY_BASIC_INFORMATION lpBuffer,
        uint dwLength
    );

    // Process Access Rights
    [Flags]
    public enum ProcessAccessFlags : uint
    {
        PROCESS_VM_READ = 0x0010,
        PROCESS_VM_WRITE = 0x0020,
        PROCESS_VM_OPERATION = 0x0008,
        PROCESS_QUERY_INFORMATION = 0x0400,
        PROCESS_ALL_ACCESS = 0x001F0FFF
    }

    // Structure to receive memory information
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    public static void GetProcessInformation(int processId)
    {
        // Request all access rights to inspect the process thoroughly
        var processHandle = OpenProcess(
            ProcessAccessFlags.PROCESS_VM_READ | ProcessAccessFlags.PROCESS_VM_WRITE | 
            ProcessAccessFlags.PROCESS_QUERY_INFORMATION,
            false,
            processId
        );

        if (processHandle == IntPtr.Zero)
        {
            Console.WriteLine("Failed to open process. Error: " + Marshal.GetLastWin32Error());
            return;
        }

        Console.WriteLine($"Handle to process {processId} acquired successfully.");
        Console.WriteLine($"Process Handle: 0x{processHandle.ToInt64():X}");

        try
        {
            MEMORY_BASIC_INFORMATION mbi = new MEMORY_BASIC_INFORMATION();
            IntPtr address = IntPtr.Zero;

            // Query memory regions of the process
            Console.WriteLine("\nMemory Regions:");
            while (VirtualQueryEx(processHandle, address, out mbi, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) != 0)
            {
                Console.WriteLine($"Base Address: 0x{mbi.BaseAddress.ToInt64():X}");
                Console.WriteLine($"Allocation Base: 0x{mbi.AllocationBase.ToInt64():X}");
                Console.WriteLine($"Region Size: 0x{mbi.RegionSize.ToInt64():X}");
                Console.WriteLine($"State: 0x{mbi.State:X}");
                Console.WriteLine($"Protect: 0x{mbi.Protect:X}");
                Console.WriteLine($"Type: 0x{mbi.Type:X}");
                Console.WriteLine();
                Console.WriteLine($"State: {mbi.State}");
                Console.WriteLine($"Protect: {mbi.Protect}");
                Console.WriteLine($"Type: {mbi.Type}");
                Console.WriteLine();

                // Move to the next memory region
                address = new IntPtr(mbi.BaseAddress.ToInt64() + mbi.RegionSize.ToInt64());
            }
        }
        finally
        {
            // Clean up
            CloseHandle(processHandle);
        }
    }

    // CloseHandle function from kernel32.dll
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    static void Main(string[] args)
    {
        Console.Write("Enter the Process ID to inspect: ");
        if (int.TryParse(Console.ReadLine(), out int processId))
        {
            GetProcessInformation(processId);
        }
        else
        {
            Console.WriteLine("Invalid Process ID.");
        }
    }
}
