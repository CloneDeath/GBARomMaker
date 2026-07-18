#!/bin/bash
set -euo pipefail

dotnet build
dotnet "GBARomMaker/bin/Debug/net10.0/GBARomMaker.dll" "Examples/RedPixel/bin/Debug/net10.0/RedPixel.dll" "Examples/RedPixel.gba"
dotnet "GBARomMaker/bin/Debug/net10.0/GBARomMaker.dll" "Examples/RedLine/bin/Debug/net10.0/RedLine.dll" "Examples/RedLine.gba"
dotnet "GBARomMaker/bin/Debug/net10.0/GBARomMaker.dll" "Examples/ColoredSprites/bin/Debug/net10.0/ColoredSprites.dll" "Examples/ColoredSprites.gba"
