rule RestrictedAdmin
{
    meta:
        description = "The TypeLibGUID present in a .NET binary maps directly to the ProjectGuid found in the '.csproj' file of a .NET project."
        author = "Will Schroeder (@harmj0y)"
    strings:
        $typelibguid = "79F11FC0-ABFF-4E1F-B07C-5D65653D8952" ascii nocase wide
    condition:
        uint16(0) == 0x5A4D and $typelibguid
}