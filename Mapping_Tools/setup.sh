#!/bin/bash

DISTROBOX_NAME="avalonia_mapping_tools-bullseye"
DISTROBOX_VERSION="debian:bullseye"

check_distrobox_installed ()
{
    DISTROBOX=`command -v distrobox`
    if [ "$DISTROBOX" == "" ]; then
        echo -e "Distrobox is not installed.\nExitting"
        exit 2
    fi
}

remove_container ()
{
    echo "Removing $DISTROBOX_NAME"
    distrobox stop $DISTROBOX_NAME -Y
    distrobox rm $DISTROBOX_NAME -Y
    exit
}

check_container ()
{
    # check if distrobox is already created
    DISTROBOX=`distrobox ls | grep "$DISTROBOX_NAME"`
    if [ "$DISTROBOX" != "" ]; then
        echo -e "Distrobox container ${DISTROBOX_NAME} already exists.\nExitting"
        exit 2
    fi
}

create_container ()
{
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
}

check_distrobox_installed

if [ "$1" == "remove" ]; then
    remove_container
fi

check_container
create_container
