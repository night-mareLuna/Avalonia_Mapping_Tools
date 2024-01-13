# Avalonia Mapping Tools 

Very early (but usable) port of [Mapping Tools](https://github.com/OliBomby/Mapping_Tools) to [AvaloniaUI](https://www.avaloniaui.net/).
This is made for the purpose of running Mapping Tools natively on linux based systems. This also makes it possible to run on MacOS, however I have no way to run and test for any MacOS system.

All tool specific settings should function exactly the same as original Mapping Tools. Any issues encountered with tools should be reported to OliBomby unless issues are specific to Avalonia Mapping Tools.

## Currently Implimented Tools and Features
- Automatic and periodic backups
- [Map Cleaner](https://github.com/OliBomby/Map-Cleaner) by [OliBomby](https://github.com/OliBomby)
- Hitsound Copier by [OliBomby](https://github.com/OliBomby)
- Hitsound Preview Helper by [OliBomby](https://github.com/OliBomby) 


## Requirements
- [.NET Runtime 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0), please follow instructions based on your distro for installation.
- [Sox](https://github.com/chirlu/sox) to preview samples in Hitsound Studio.

## Installing sox
### Arch
`pacman -Syu --needed sox wavpack libvorbis`
### Fedora
`dnf install sox`
### Debian
`apt install sox libsox-fmt-all`

## Build Requirements
- [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0), please follow instructions based on your distro for installation.

