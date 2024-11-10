using System;
using System.Runtime.InteropServices;

namespace DriverService
{
    class Program
    {
        // Import necessary Windows API functions for service management
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern IntPtr CreateService(
            IntPtr hSCManager,
            string lpServiceName,
            string lpDisplayName,
            uint dwDesiredAccess,
            uint dwServiceType,
            uint dwStartType,
            uint dwErrorControl,
            string lpBinaryPathName,
            string lpLoadOrderGroup,
            IntPtr lpdwTagId,
            string lpDependencies,
            string lpServiceStartName,
            string lpPassword);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool StartService(IntPtr hService, int dwNumServiceArgs, IntPtr lpServiceArgVectors);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CloseServiceHandle(IntPtr hSCObject);

        private const uint SC_MANAGER_ALL_ACCESS = 0xF003F;
        private const uint SERVICE_ALL_ACCESS = 0xF01FF;
        private const uint SERVICE_KERNEL_DRIVER = 0x00000001;
        private const uint SERVICE_DEMAND_START = 0x00000003;
        private const uint SERVICE_ERROR_NORMAL = 0x00000001;

        static void Main(string[] args)
        {
            try
            {
                string serviceName = "WinRing0_1_2_0";
                string displayName = "WinRing0 Kernel Driver";
                string binaryPath = @"C:\<path to driver>\WinRing0x64.sys"; // Ensure the path is correct

                Console.WriteLine("Attempting to open Service Control Manager...");
                IntPtr scManager = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
                if (scManager == IntPtr.Zero)
                {
                    Console.WriteLine($"Failed to open Service Control Manager. Error: {Marshal.GetLastWin32Error()}");
                    return;
                }
                Console.WriteLine("Service Control Manager opened successfully.");

                Console.WriteLine("Attempting to create the service...");
                IntPtr service = CreateService(
                    scManager,
                    serviceName,
                    displayName,
                    SERVICE_ALL_ACCESS,
                    SERVICE_KERNEL_DRIVER,
                    SERVICE_DEMAND_START,
                    SERVICE_ERROR_NORMAL,
                    binaryPath,
                    null,
                    IntPtr.Zero,
                    null,
                    null,
                    null);

                if (service == IntPtr.Zero)
                {
                    Console.WriteLine($"Failed to create the service. Error: {Marshal.GetLastWin32Error()}");
                    CloseServiceHandle(scManager);
                    return;
                }
                Console.WriteLine("Service created successfully.");

                Console.WriteLine("Attempting to start the service...");
                bool serviceStarted = StartService(service, 0, IntPtr.Zero);
                if (!serviceStarted)
                {
                    Console.WriteLine($"Failed to start the service. Error: {Marshal.GetLastWin32Error()}");
                }
                else
                {
                    Console.WriteLine("Service started successfully.");
                }

                CloseServiceHandle(service);
                CloseServiceHandle(scManager);

                Console.WriteLine("Operation completed. Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
