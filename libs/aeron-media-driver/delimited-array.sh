#!/bin/bash

function PrintSep {
  local delimiter=${1:?missing delimiter argument}
  python3 -c "import sys; print(*sys.argv[1:], sep='${delimiter}', end='')" "${@:2}"
}
export -f PrintSep

function ReadSep {
  local array_name=${1:?missing array name argument}
  local delimiter=${2:?missing delimiter string argument}
  readarray -t -d "" "$array_name"
}
export -f ReadSep

function Print0 {
  python3 -c "import sys; print(*sys.argv[1:], sep='\x00', end='')" "$@"
}
export -f Print0

function Read0 {
  local array_name=${1:?missing array name argument}
  readarray -t -d "" "$array_name"
}
export -f Read0

function PrintNewline {
  printf "%s\n" "$@"
}
export -f PrintNewline

function ReadNewline {
  local array_name=${1:?missing array name argument}
  readarray -t "$array_name"
}
export -f ReadNewline
