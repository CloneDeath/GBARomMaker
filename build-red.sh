#!/bin/bash
set -euo pipefail

dotnet build
pushd RedPixel/
dotnet build -v:n --tl:off
popd
