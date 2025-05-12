#!/bin/bash

# https://stackoverflow.com/a/77663806/11045433
AeronDriverDirectory=$(dirname "$( readlink -f "${BASH_SOURCE[0]:-"$( command -v -- "$0" )"}" )")

echo Media Driver Started...
java -cp "$AeronDriverDirectory/media-driver.jar" io.aeron.driver.MediaDriver
echo Media Driver Stopped.
