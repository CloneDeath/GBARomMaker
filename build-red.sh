#!/bin/bash
set -euo pipefail

#dotnet build
#pushd RedPixel/
#dotnet build -v:n --tl:off
#popd

dotnet build
dotnet "GBARomMaker/bin/Debug/net10.0/GBARomMaker.dll" "/home/nicholas/Projects/Personal/gba/GBARomMaker/RedPixel/bin/Debug/net10.0/RedPixel.dll" "RedPixel.gba"
