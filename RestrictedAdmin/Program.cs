using System;
using System.Threading;
using System.Management;
using System.Text.RegularExpressions;

/*
    Author: @harmj0y
    License: BSD 3-Clause
*/

namespace RestrictedAdmin
{
    class Program
    {
        private static int GetRestrictedAdminRegistryValue(string computerName)
        {
            /* 
             * Returns the DisableRestrictedAdmin registry setting for a remote "computerName" using WMI's StdRegProv
             * 
             *      Return value == 0  -> RestrictedAdmin is enabled
             *      Return value == 1  -> RestrictedAdmin is disabled
             *      Return value == -1 -> RestrictedAdmin value is cleared, so disabled behavior
             * 
             * Note: adapted from https://web.archive.org/web/20200212015446/http://softvernow.com/2018/09/02/using-wmi-and-c-registry-values/
             * 
             */

            int result = 0;
            ManagementScope scope = null;

            try
            {
                ConnectionOptions connection = new ConnectionOptions();
                connection.Impersonation = System.Management.ImpersonationLevel.Impersonate;

                // optional: explicit credentials
                //connection.Username = "userName";
                //connection.Password = "password";
                //connection.Authority = "NTLMDOMAIN:MY_DOMAIN";

                // connect to the remote management scope
                scope = new ManagementScope($"\\\\{computerName}\\root\\default", connection);
                scope.Connect();

                // instantiate the StdRegProv class for remote registry interaction
                ManagementClass registry = new ManagementClass(scope, new ManagementPath("StdRegProv"), null);

                // grab the DisableRestrictedAdmin value, if it exists
                ManagementBaseObject inParams = registry.GetMethodParameters("GetDWORDValue");
                inParams["sSubKeyName"] = @"SYSTEM\CurrentControlSet\Control\Lsa";
                inParams["sValueName"] = "DisableRestrictedAdmin";
                ManagementBaseObject outParams = registry.InvokeMethod("GetDWORDValue", inParams, null);
                result = (int)(UInt32)outParams["uValue"];
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Object reference not set to an instance of an object"))
                {
                    result = -1;
                }
                else
                {
                    Console.WriteLine($"\n[X] Error: {e.Message}\n");
                    result = -2;
                }
            }
            return result;
        }

        private static void SetRestrictedAdminRegistryValue(string computerName, int value)
        {
            /* 
             * Sets (or clears) the DisableRestrictedAdmin registry setting on a remote "computerName" using WMI's StdRegProv
             * 
             *      value == 0  -> enable RestrictedAdmin (set DisableRestrictedAdmin set to 0)
             *      value == 1  -> enable RestrictedAdmin (set DisableRestrictedAdmin set to 1)
             *      value == -1 -> clear DisableRestrictedAdmin value (so disabled behavior)
             * 
             * Note: adapted from https://web.archive.org/web/20200212015446/http://softvernow.com/2018/09/02/using-wmi-and-c-registry-values/
             * 
             */

            ManagementScope scope = null;

            try
            {
                ConnectionOptions connection = new ConnectionOptions();
                connection.Impersonation = System.Management.ImpersonationLevel.Impersonate;

                // optional: explicit credentials
                //connection.Username = "userName";
                //connection.Password = "password";
                //connection.Authority = "NTLMDOMAIN:MY_DOMAIN";

                // connect to the remote management scope
                scope = new ManagementScope($"\\\\{computerName}\\root\\default", connection);
                scope.Connect();

                // instantiate the StdRegProv class for remote registry interaction
                ManagementClass registry = new ManagementClass(scope, new ManagementPath("StdRegProv"), null);
                
                if(value == -1)
                {
                    // if we're clearing the value
                    ManagementBaseObject inParams = registry.GetMethodParameters("DeleteValue");
                    inParams["sSubKeyName"] = @"SYSTEM\CurrentControlSet\Control\Lsa";
                    inParams["sValueName"] = @"DisableRestrictedAdmin";
                    ManagementBaseObject outParams = registry.InvokeMethod("DeleteValue", inParams, null);
                }
                else
                {
                    // otherwise set the DisableRestrictedAdmin value to what's specified
                    ManagementBaseObject inParams = registry.GetMethodParameters("SetDWORDValue");
                    inParams["sSubKeyName"] = @"SYSTEM\CurrentControlSet\Control\Lsa";
                    inParams["sValueName"] = @"DisableRestrictedAdmin";
                    inParams["uValue"] = (UInt32)value;
                    ManagementBaseObject outParams = registry.InvokeMethod("SetDWORDValue", inParams, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[X] Error: {e.Message}");
            }
        }

        public static void Usage()
        {
            Console.WriteLine("\nUsage: ");
            Console.WriteLine("\n\tCheck the DisableRestrictedAdmin value:\n\t\tRestrictedAdmin.exe <system.domain.com>\n");
            Console.WriteLine("\n\tEnabled Restricted Admin mode (set DisableRestrictedAdmin to 0):\n\t\tRestrictedAdmin.exe <system.domain.com> 0\n");
            Console.WriteLine("\n\tDisable Restricted Admin mode (set DisableRestrictedAdmin to 1):\n\t\tRestrictedAdmin.exe <system.domain.com> 1\n");
            Console.WriteLine("\n\tClear the Restricted Admin mode setting completely:\n\t\tRestrictedAdmin.exe <system.domain.com> clear\n");
        }

        static void Main(string[] args)
        {
            Console.WriteLine();

            if(args.Length < 1 || args.Length > 2)
            {
                Usage();
                return;
            }
            else if (args.Length == 1)
            {
                Regex regex = new Regex(@"-h");
                Match match = regex.Match(args[0]);
                if (match.Success)
                {
                    Usage();
                    return;
                }

                int value = GetRestrictedAdminRegistryValue(args[0]);
                if (value == -2)
                {
                    // fatal error
                    Console.WriteLine($"[!] Error\n");
                    return;
                }
                else if (value == -1)
                {
                    Console.WriteLine($"[*] DisableRestrictedAdmin key not set\n");
                }
                else
                {
                    Console.WriteLine($"[*] DisableRestrictedAdmin value: {value}\n");
                }
            }
            else
            {
                int value = GetRestrictedAdminRegistryValue(args[0]);
                if (value == -2)
                {
                    // fatal error
                    Console.WriteLine($"[!] Error\n");
                    return;
                }
                else if (value == -1)
                {
                    Console.WriteLine($"[*] DisableRestrictedAdmin key not set");
                }
                else
                {
                    Console.WriteLine($"[*] Old DisableRestrictedAdmin value: {value}");
                }

                if(String.Equals(args[1], "clear", StringComparison.CurrentCultureIgnoreCase)) {
                    SetRestrictedAdminRegistryValue(args[0], -1);
                    Thread.Sleep(1 * 1000);
                    value = GetRestrictedAdminRegistryValue(args[0]);
                }
                else
                {
                    int i = 0;
                    bool result = int.TryParse(args[1], out i);
                    if(result && (i == 0 || i == 1))
                    {
                        SetRestrictedAdminRegistryValue(args[0], i);
                        Thread.Sleep(1 * 1000);
                        value = GetRestrictedAdminRegistryValue(args[0]);
                    }
                    else
                    {
                        Usage();
                        return;
                    }
                }

                if (value == -2)
                {
                    // fatal error
                    Console.WriteLine($"[!] Error\n");
                    return;
                }
                else if (value == -1)
                {
                    Console.WriteLine($"[+] New DisableRestrictedAdmin value: not set");
                }
                else
                {
                    Console.WriteLine($"[+] New DisableRestrictedAdmin value: {value}");
                }
            }
            Console.WriteLine();
        }
    }
}
