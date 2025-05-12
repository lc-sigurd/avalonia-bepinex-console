#!/bin/bash

# https://stackoverflow.com/a/77663806/11045433
script_directory=$(dirname "$( readlink -f "${BASH_SOURCE[0]:-"$( command -v -- "$0" )"}" )")

# 1st positional parameter: aeron directory. Defaults to /dev/shm/aeron-USER
aeron_directory=${1:-"/dev/shm/aeron-$(whoami)"}

source "$script_directory/java-with-media-driver-classpath.sh"

PrepareJavaWithMediaDriverClasspath
echo Media Driver Started...
JavaWithMediaDriverClasspath -Daeron.dir="$aeron_directory" io.aeron.driver.MediaDriver
echo Media Driver Stopped.
