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
- [Sox](https://github.com/chirlu/sox) to preview samples in Hitsound Studio.
- [.NET Runtime 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) if you do not download the self-contained package or AppImage.

## Installing sox
### Arch
`pacman -S --needed sox wavpack libvorbis`
### Fedora
`dnf install sox`
### Debian
`apt install sox libsox-fmt-all`

## Build Requirements
- [distrobox](https://github.com/89luca89/distrobox)
The setup script will handle creating and installing build dependancies into a distrobox container.
- [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0), if you want to do more than just download source then build.

### Building with distrobox
Clone the repo, run ``setup.sh`` then run ``build.sh``.
```
git clone https://github.com/night-mareLuna/Avalonia_Mapping_Tools.git
cd Avalonia_Mapping_Tools/Mapping_Tools
chmod +x setup.sh
./setup.sh
chmod +x build.sh
./build.sh
```
Setup might take a few minutes (you only need to do this once). You can remove the distrobox container by running ``setup.sh remove``.

