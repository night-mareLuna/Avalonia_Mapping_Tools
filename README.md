# Avalonia Mapping Tools 

A basic port of [Mapping Tools](https://github.com/OliBomby/Mapping_Tools) to [AvaloniaUI](https://www.avaloniaui.net/).
This is made for the purpose of running Mapping Tools natively on linux based desktops. This theoretically makes it possible to run on MacOS, however I have no way to run and test for any MacOS system.

All tool specific settings should function exactly the same as original Mapping Tools. Any issues encountered with tools should be reported to OliBomby unless issues are specific to Avalonia Mapping Tools.

## Currently Implimented Tools and Features
- Automatic and periodic backups
- Automatically setup and run [gosumemory](https://github.com/l3lackShark/gosumemory) to fetch the currently selected beatmap directly from osu!
- [Map Cleaner](https://github.com/OliBomby/Map-Cleaner) by [OliBomby](https://github.com/OliBomby)
- Hitsound Copier by [OliBomby](https://github.com/OliBomby)
- Hitsound Preview Helper by [OliBomby](https://github.com/OliBomby) 
- Hitsound Studio by [OliBomby](https://github.com/OliBomby) 
- Rhythm Guide by [OliBomby](https://github.com/OliBomby) 
- Metadata Manager by [OliBomby](https://github.com/OliBomby) 
- Slider Merger by [OliBomby](https://github.com/OliBomby)
- Property Transformer by [OliBomby](https://github.com/OliBomby)


## Requirements
- [.NET Runtime 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0), please follow instructions based on your distro for installation (There is a separate build for those who do not want to (or cannot) install .NET 8.0).
- [Sox](https://github.com/chirlu/sox) to preview samples in Hitsound Studio.

## Installing sox
### Arch
`pacman -S --needed sox wavpack libvorbis`
### Fedora
`dnf install sox`
### Debian
`apt install sox libsox-fmt-all`

## Build Requirements
- [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0), please follow instructions based on your distro for installation.

