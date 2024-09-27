#!/bin/bash

DISTROBOX=`command -v distrobox`
if [ "$DISTROBOX" == "" ]; then
    echo -e "Distrobox is not installed.\nExitting"
    exit
fi

DISTROBOX_NAME="avalonia_mapping_tools-bullseye"
DISTROBOX_VERSION="debian:bullseye"

# remove container
if [ "$1" == "remove" ]; then
    echo "Removing $DISTROBOX_NAME"
    distrobox stop $DISTROBOX_NAME -Y
    distrobox rm $DISTROBOX_NAME -Y
    exit
fi

# check if distrobox is already created
DISTROBOX=`distrobox ls | grep "$DISTROBOX_NAME"`
if [ "$DISTROBOX" != "" ]; then
    echo -e "Distrobox container ${DISTROBOX_NAME} already exists.\nExitting"
    exit
fi

echo "Creating distrobox container ${DISTROBOX_NAME} using ${DISTROBOX_VERSION}"
distrobox create -n $DISTROBOX_NAME -i $DISTROBOX_VERSION -Y

echo "Installing dependancies"
distrobox enter $DISTROBOX_NAME -- bash -c "
    wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb;
    sudo dpkg -i packages-microsoft-prod.deb;
    rm packages-microsoft-prod.deb;

    sudo apt-get update && \
    sudo apt-get install -y dotnet-sdk-8.0 file fuse"

distrobox stop $DISTROBOX_NAME -Y

echo -e "\nDone!"