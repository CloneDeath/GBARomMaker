#!/bin/bash
set -euo pipefail

dotnet build
dotnet "CILBoy/bin/Debug/net10.0/CILBoy.dll" "Examples/01-RedPixel/bin/Debug/net10.0/RedPixel.dll" "Examples/01-RedPixel.gba"
dotnet "CILBoy/bin/Debug/net10.0/CILBoy.dll" "Examples/02-RedLine/bin/Debug/net10.0/RedLine.dll" "Examples/02-RedLine.gba"
dotnet "CILBoy/bin/Debug/net10.0/CILBoy.dll" "Examples/03-RedSquare/bin/Debug/net10.0/RedSquare.dll" "Examples/03-RedSquare.gba"
dotnet "CILBoy/bin/Debug/net10.0/CILBoy.dll" "Examples/04-ColorFill/bin/Debug/net10.0/ColorFill.dll" "Examples/04-ColorFill.gba"
dotnet "CILBoy/bin/Debug/net10.0/CILBoy.dll" "Examples/05-ColoredSprites/bin/Debug/net10.0/ColoredSprites.dll" "Examples/05-ColoredSprites.gba"
dotnet "CILBoy/bin/Debug/net10.0/CILBoy.dll" "Examples/06-ConsoleWrite/bin/Debug/net10.0/ConsoleWrite.dll" "Examples/06-ConsoleWrite.gba"
dotnet "CILBoy/bin/Debug/net10.0/CILBoy.dll" "Examples/07-SpriteArray/bin/Debug/net10.0/SpriteArray.dll" "Examples/07-SpriteArray.gba"
dotnet "CILBoy/bin/Debug/net10.0/CILBoy.dll" "Examples/08-Input/bin/Debug/net10.0/Input.dll" "Examples/08-Input.gba"
dotnet "CILBoy/bin/Debug/net10.0/CILBoy.dll" "Examples/09-Exceptions/bin/Debug/net10.0/Exceptions.dll" "Examples/09-Exceptions.gba" --show-cil --show-arm
