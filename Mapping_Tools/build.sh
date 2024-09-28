#!/bin/bash

BUILD_APPIMAGE=""

# check if in correct directory
if [ "`basename $PWD`" != "Mapping_Tools" ]; then
    echo -e "Make sure you are in Mapping_Tools directory before running build.sh!\nExitting"
    exit
fi

# check if distrobox is installed
DISTROBOX=`command -v distrobox`
if [ "$DISTROBOX" == "" ]; then
    echo -e "Distrobox is not installed.\nExitting"
    exit
fi

# check if distrobox setup has been done
DISTROBOX_NAME="avalonia_mapping_tools-bullseye"
DISTROBOX=`distrobox ls | grep "$DISTROBOX_NAME"`
if [ "$DISTROBOX" == "" ]; then
    echo -e "Distrobox has not been setup yet! Run setup.sh first.\nExitting"
    exit
fi

CONTAINED=""
BUILDPATH="${PWD}/bin/Release/net8.0/linux-x64/publish/"

if [ "$1" == "false" ]; then  
    CONTAINED="$1"
    echo "Not building self contained binary. .NET 8.0 will be needed to run this application."
else
    CONTAINED="true"
    echo "Building self contained binary."
fi

# prevent build from bugging out when old build files already exist for some reason
if [ "$BUILDPATH" != "" ]; then
    rm -rf $BUILDPATH/*
else
    echo -e "Exiting to prevent a catastrophic disaster."
    exit
fi

distrobox enter $DISTROBOX_NAME -- dotnet publish --self-contained $CONTAINED

# check failure
if ! test -e $BUILDPATH/Mapping_Tools ; then
    echo -e "\nCompilation failed!"
    exit
fi

if [ "$CONTAINED" != "true" ]; then
    echo -e "\nAppImage will only be built with a self-contained binary"
    xdg-open $BUILDPATH
    exit
elif [ "$BUILD_APPIMAGE" == "false" ]; then
    echo "\nAppImage build has been set to disabled"
    xdg-open $BUILDPATH
    exit
fi

## BUILD APPIMAGE 
echo "Building AppImage"
mkdir -p $BUILDPATH/AppDir/usr/bin
cp $BUILDPATH/* $BUILDPATH/AppDir/usr/bin
cp $PWD/Data/mt_logo_256.png $BUILDPATH/AppDir

wget "https://github.com/AppImage/AppImageKit/releases/latest/download/AppRun-x86_64" --directory-prefix $BUILDPATH/AppDir/
mv $BUILDPATH/AppDir/AppRun-x86_64 $BUILDPATH/AppDir/AppRun

chmod +x $BUILDPATH/AppDir/AppRun

echo '# Desktop Entry Specification: https://standards.freedesktop.org/desktop-entry-spec/desktop-entry-spec-latest.html
[Desktop Entry]
Type=Application
Name=Avalonia_Mapping_Tools
Comment=Collection of tools for manipulating osu! beatmaps.
Icon=mt_logo_256
Exec=Mapping_Tools
Path=/
Terminal=true
Categories=Utility;' > $BUILDPATH/AppDir/Mapping_Tools.desktop

if ! test -e $PWD/bin/appimagetool-x86_64.AppImage; then
    wget "https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage" --directory-prefix $PWD/bin/
    chmod +x $PWD/bin/appimagetool-x86_64.AppImage
fi

distrobox enter $DISTROBOX_NAME -- $PWD/bin/appimagetool-x86_64.AppImage $BUILDPATH/AppDir
mv Avalonia_Mapping_Tools-x86_64.AppImage $BUILDPATH/Avalonia_Mapping_Tools-x86_64.AppImage

distrobox stop $DISTROBOX_NAME -Y

xdg-open $BUILDPATH
