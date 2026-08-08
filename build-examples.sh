#!/bin/bash
set -euo pipefail

dotnet build
dotnet "GBARomMaker/bin/Debug/net10.0/GBARomMaker.dll" "Examples/RedPixel/bin/Debug/net10.0/RedPixel.dll" "Examples/01-RedPixel.gba"
dotnet "GBARomMaker/bin/Debug/net10.0/GBARomMaker.dll" "Examples/RedLine/bin/Debug/net10.0/RedLine.dll" "Examples/02-RedLine.gba"
dotnet "GBARomMaker/bin/Debug/net10.0/GBARomMaker.dll" "Examples/RedSquare/bin/Debug/net10.0/RedSquare.dll" "Examples/03-RedSquare.gba"
dotnet "GBARomMaker/bin/Debug/net10.0/GBARomMaker.dll" "Examples/ColorFill/bin/Debug/net10.0/ColorFill.dll" "Examples/04-ColorFill.gba"
dotnet "GBARomMaker/bin/Debug/net10.0/GBARomMaker.dll" "Examples/ColoredSprites/bin/Debug/net10.0/ColoredSprites.dll" "Examples/05-ColoredSprites.gba"
dotnet "GBARomMaker/bin/Debug/net10.0/GBARomMaker.dll" "Examples/SpriteArray/bin/Debug/net10.0/SpriteArray.dll" "Examples/06-SpriteArray.gba" --show-cil --show-arm
