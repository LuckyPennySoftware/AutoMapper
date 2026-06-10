#!/bin/bash

version="$1"
echo "$version"

# Split version string into components
IFS='.'
set -- "$version"
versionNumbers=("$@")

major="${versionNumbers[0]}"
minor="${versionNumbers[1]}"
patch="${versionNumbers[2]}"

# Determine previous major version if minor and patch are 0
if [ "$minor" -eq 0 ] && [ "$patch" -eq 0 ]; then
    oldVersion=$((major - 1))
else
    oldVersion=$major
fi

oldVersion="$oldVersion.0.0"
echo "$oldVersion"

# Fetch and extract AutoMapper package
rm -rf ../LastMajorVersionBinary
curl -sSL "https://globalcdn.nuget.org/packages/automapper.$oldVersion.nupkg" \
    --create-dirs -o "../LastMajorVersionBinary/automapper.$oldVersion.nupkg"

unzip -j "../LastMajorVersionBinary/automapper.$oldVersion.nupkg" "lib/net*.0/AutoMapper.dll" -d ../LastMajorVersionBinary