#!/bin/bash

WORKSPACE=.
LUBAN_DLL=$WORKSPACE/Tools/Luban/Luban.dll

dotnet $LUBAN_DLL \
    -t all \
    -c cs-simple-json \
    -d json2 \
    --conf $WORKSPACE/luban.conf \
    -x outputDataDir=OutputDataTables \
    -x outputCodeDir=OutputCodes