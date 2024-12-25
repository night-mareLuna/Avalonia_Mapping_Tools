#!/bin/bash

BUILD_APPIMAGE=""  # set to "false" to disable appimage build
CONTAINED=""
BUILDPATH="${PWD}/bin/Release/net8.0/linux-x64/publish/"

# check if in correct directory
if [ "`basename $PWD`" != "Mapping_Tools" ]; then
    echo -e "Make sure you are in Mapping_Tools directory before running build.sh!\nExitting"
    exit 1
fi

pre_checks ()
{
    # check if distrobox is installed
    DISTROBOX=`command -v distrobox`
    if [ "$DISTROBOX" == "" ]; then
        echo -e "Distrobox is not installed.\nExitting"
        exit 2
    fi

    # check if distrobox setup has been done
    DISTROBOX_NAME="avalonia_mapping_tools-bullseye"
    DISTROBOX=`distrobox ls | grep "$DISTROBOX_NAME"`
    if [ "$DISTROBOX" == "" ]; then
        echo -e "Distrobox has not been setup yet! Run setup.sh first.\nExitting"
        exit 2
    fi
}

set_contained ()
{
    if [ "$1" == "false" ]; then  
        CONTAINED="$1"
        echo "Not building self contained binary. .NET 8.0 will be needed to run this application."
    else
        CONTAINED="true"
        echo "Building self contained binary."
    fi
}

clean_buildpath ()
{
    # prevent build from bugging out when old build files already exist for some reason
    if [ "$BUILDPATH" != "" ]; then
        rm -rf $BUILDPATH/*
    else
        echo -e "Exiting to prevent a catastrophic disaster."
        exit 1
    fi
}

dotnet_build ()
{
    distrobox enter $DISTROBOX_NAME -- dotnet publish --self-contained $CONTAINED
    # check failure
    if ! test -e $BUILDPATH/Mapping_Tools ; then
        echo -e "\nCompilation failed!"
        distrobox stop $DISTROBOX_NAME -Y
        exit 1
    fi

    if [ "$CONTAINED" != "true" ]; then
        echo -e "\nAppImage will only be built with a self-contained binary"
    elif [ "$BUILD_APPIMAGE" == "false" ]; then
        echo "\nAppImage build has been set to disabled"
    else
        appimage_build
    fi

    distrobox stop $DISTROBOX_NAME -Y
    xdg-open $BUILDPATH
}

appimage_build ()
{
    echo "Building AppImage"
    mkdir -p $BUILDPATH/AppDir/usr/bin
    cp $BUILDPATH/* $BUILDPATH/AppDir/usr/bin
    cp $PWD/Data/mt_logo_256.png $BUILDPATH/AppDir

    wget "https://github.com/AppImage/AppImageKit/releases/latest/download/AppRun-x86_64" --directory-prefix $BUILDPATH/AppDir/
    mv $BUILDPATH/AppDir/AppRun-x86_64 $BUILDPATH/AppDir/AppRun

    chmod +x $BUILDPATH/AppDir/AppRun

    echo '[Desktop Entry]
Type=Application
Name=Avalonia_Mapping_Tools
Comment=Collection of tools for manipulating osu! beatmaps.
Icon=mt_logo_256
Exec=Mapping_Tools
Categories=Utility;' > $BUILDPATH/AppDir/Mapping_Tools.desktop

    if ! test -e $PWD/bin/appimagetool-x86_64.AppImage; then
        wget "https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage" --directory-prefix $PWD/bin/
        chmod +x $PWD/bin/appimagetool-x86_64.AppImage
    fi

    distrobox enter $DISTROBOX_NAME -- $PWD/bin/appimagetool-x86_64.AppImage $BUILDPATH/AppDir
    mv Avalonia_Mapping_Tools-x86_64.AppImage $BUILDPATH/Avalonia_Mapping_Tools-x86_64.AppImage
}

pre_checks
set_contained $1
clean_buildpath
dotnet_build
