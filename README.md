# RestrictedAdmin

Quick and dirty C# program that remotely enables "Restricted Admin Mode".

Restricted Admin Mode was introduced in Windows 8.1 as an attempt to prevent credential exposure via RDP. While well intentioned, this unfortunately introduced the ability to pass-the-hash to RDP.

While Restricted Admin Mode is not enabled by default on systems, we can enable it by setting the value of `DisableRestrictedAdmin` to 0 at `HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Lsa`. In order to do this remotely, we could use remote registry, however this is not always enabled on systems (particularly workstations). Instead, we can use the [StdRegProv WMI class](https://docs.microsoft.com/en-us/previous-versions/windows/desktop/regprov/stdregprov) to flip this value remotely. This approach was later expanded by @airzero24 in his [WMIReg](https://github.com/airzero24/WMIReg) project.

The TypeLib GUID of Certify is **79F11FC0-ABFF-4E1F-B07C-5D65653D8952**. This is reflected in the Yara rules currently in this repo.

**I did not invent or figure out any of this**. For more information and references on the work this was built on, see the **References** section at the bottom of this README.


## Usage


    C:\Tools>RestrictedAdmin.exe


    Usage:

            Check the DisableRestrictedAdmin value:
                    RestrictedAdmin.exe <system.domain.com>


            Enabled Restricted Admin mode (set DisableRestrictedAdmin to 0):
                    RestrictedAdmin.exe <system.domain.com> 0


            Disable Restricted Admin mode (set DisableRestrictedAdmin to 1):
                    RestrictedAdmin.exe <system.domain.com> 1


            Clear the Restricted Admin mode setting completely:
                    RestrictedAdmin.exe <system.domain.com> clear



## References

* [Details on using this for PTH from Portcullis Labs.](https://labs.portcullis.co.uk/blog/new-restricted-admin-feature-of-rdp-8-1-allows-pass-the-hash/)
* [PTH + RDP w/ restricted admin mode in Kali.](https://www.kali.org/blog/passing-hash-remote-desktop/)
* [F-Secure has a post about offensively disabling Restricted Admin Mode.](https://labs.f-secure.com/blog/undisable/).
* [Some Restricted Admin Mode details from Microsoft.](https://docs.microsoft.com/en-us/archive/blogs/kfalde/restricted-admin-mode-for-rdp-in-windows-8-1-2012-r2)
* The StdRegProv approach was adapted from [this post](https://web.archive.org/web/20200212015446/http://softvernow.com/2018/09/02/using-wmi-and-c-registry-values/).

