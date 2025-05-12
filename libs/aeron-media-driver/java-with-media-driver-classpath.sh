#!/bin/bash

# define constants
AERON_GROUP="io.aeron"
AERON_DRIVER_ARTIFACT_ID="aeron-driver"
AERON_DRIVER_ARTIFACT_VERSION="1.47.5"
JAVA_VERSION="17"

# https://stackoverflow.com/a/77663806/11045433
script_directory=$(dirname "$( readlink -f "${BASH_SOURCE[0]:-"$( command -v -- "$0" )"}" )")

# https://serverfault.com/a/1100799
java_exe=$(update-alternatives --list java | grep "java-$JAVA_VERSION")

source "$script_directory/delimited-array.sh"
source "$script_directory/maven-util.sh"

function PrepareJavaWithMediaDriverClasspath() {
  local media_driver_classpath_array
  Read0 media_driver_classpath_array < <(
    ResolveClasspathAndEnsureExists \
      "$AERON_GROUP" \
      "$AERON_DRIVER_ARTIFACT_ID" \
      "$AERON_DRIVER_ARTIFACT_VERSION" \
      "$script_directory/classpath.txt.local"
  )

  MEDIA_DRIVER_CLASSPATH="$(PrintSep ":" "${media_driver_classpath_array[@]}")"
}
export -f PrepareJavaWithMediaDriverClasspath

function JavaWithMediaDriverClasspath() {
  "$java_exe" --add-exports java.base/jdk.internal.misc=ALL-UNNAMED -cp "$MEDIA_DRIVER_CLASSPATH" "$@"
}
export -f JavaWithMediaDriverClasspath
