#!/bin/bash

# https://stackoverflow.com/a/77663806/11045433
AeronDriverDirectory=$(dirname "$( readlink -f "${BASH_SOURCE[0]:-"$( command -v -- "$0" )"}" )")

# https://serverfault.com/a/1100799
Java=$(update-alternatives --list java | grep "java-8")

# 1st positional parameter: aeron directory. Defaults to /dev/shm/aeron-USER
AeronDir=${1:-"/dev/shm/aeron-$(whoami)"}

echo Media Driver Started...
$Java -cp "$AeronDriverDirectory/media-driver.jar" -Daeron.dir="$AeronDir" io.aeron.driver.MediaDriver
echo Media Driver Stopped.
