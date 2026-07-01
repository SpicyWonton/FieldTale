#!/bin/bash

[ -d Luban ] && rm -rf Luban

dotnet build  ./src/Luban/Luban.csproj -c Release -o Luban