#!/bin/bash

# https://stackoverflow.com/a/77663806/11045433
script_directory=$(dirname "$( readlink -f "${BASH_SOURCE[0]:-"$( command -v -- "$0" )"}" )")

source "$script_directory/delimited-array.sh"

function GroupPathFromGroupName() {
  local group="${1:?missing group name argument}"
  echo "$group" | sed 's/\./\//g'
}

function GetArtifactFromMaven() {
  local group="${1:?missing group name argument}"
  local artifact="${2:?missing artifact ID argument}"
  local version="${3:?missing artifact version argument}"

  echo "Fetching artifacts from Maven..." >&2
  mvn dependency:get -Dartifact="$group":"$artifact":"$version" >&2
}
export -f GetArtifactFromMaven

function ResolveClasspath() {
  local group="${1:?missing group name argument}"
  local artifact="${2:?missing artifact ID argument}"
  local version="${3:?missing artifact version argument}"

  local group_path
  group_path="$(GroupPathFromGroupName "$group")"
  local artifact_file_stem="$HOME/.m2/repository/$group_path/$artifact/$version/$artifact-$version"
  local artifact_jar="$artifact_file_stem".jar
  local artifact_pom="$artifact_file_stem".pom

  echo "Building classpath using Maven..." >&2
  local classpath_file
  classpath_file="$(mktemp)"
  mvn -f "$artifact_pom" dependency:build-classpath \
    -Dmdep.includeScope=runtime \
    -Dmdep.outputFile="$classpath_file" \
    -Dmdep.pathSeparator=$'\n' >&2
  readarray -t classpath < "$classpath_file"
  unlink "$classpath_file"
  classpath+=("$artifact_jar")
  Print0 "${classpath[@]}"
}
export -f ResolveClasspath

function ClasspathCacheFile() {
  local group="${1:?missing group name argument}"
  local artifact="${2:?missing artifact ID argument}"
  local version="${3:?missing artifact version argument}"
  local classpath_dir="${4:?missing classpath cache directory argument}"

  echo "$classpath_dir/$group:$artifact:$version.txt"
}

function CachedResolveClasspath() {
  local group="${1:?missing group name argument}"
  local artifact="${2:?missing artifact ID argument}"
  local version="${3:?missing artifact version argument}"
  local classpath_dir="${4:?missing classpath cache directory argument}"

  local classpath_file
  classpath_file=$(ClasspathCacheFile "$group" "$artifact" "$version" "$classpath_dir")

  if [ -e "$classpath_file" ]; then
    ReadNewline classpath < "$classpath_file"
  else
    Read0 classpath < <(ResolveClasspath "$group" "$artifact" "$version")
    mkdir -p "$classpath_dir"
    PrintNewline "${classpath[@]}" > "$classpath_file"
  fi

  Print0 "${classpath[@]}"
}
export -f CachedResolveClasspath

function TestAllClasspathExists() {
  local classpath_entry
  for classpath_entry in "$@"; do
    # if entry contains a wildcard, ignore it
    if [[ "$classpath_entry" == *"\*"* ]]; then continue; fi
    # if entry exists, good
    if [ -e "$classpath_entry" ]; then continue; fi
    # otherwise, bad
    return 1
  done
  return 0
}
export -f TestAllClasspathExists

function ResolveClasspathAndEnsureExists() {
  local group="${1:?missing group name argument}"
  local artifact="${2:?missing artifact ID argument}"
  local version="${3:?missing artifact version argument}"
  local classpath_dir="${4:?missing classpath cache directory argument}"

  Read0 classpath < <(CachedResolveClasspath "$group" "$artifact" "$version" "$classpath_dir")
  if TestAllClasspathExists "${classpath[@]}"; then
    Print0 "${classpath[@]}"
    return
  fi

  GetArtifactFromMaven "$group" "$artifact" "$version"
  Read0 classpath < <(ResolveClasspath "$group" "$artifact" "$version")
  local classpath_file
  classpath_file=$(ClasspathCacheFile "$group" "$artifact" "$version" "$classpath_dir")
  mkdir -p "$classpath_dir"
  PrintNewline "${classpath[@]}" > "$classpath_file"

  if ! TestAllClasspathExists "${classpath[@]}"; then
    echo "Some entries in the resolved classpath don't exist, something is wrong" >&2
    return 1
  fi

  Print0 "${classpath[@]}"
}
export -f ResolveClasspathAndEnsureExists
