set WORKSPACE=.
set LUBAN_DLL=%WORKSPACE%\Tools\Luban\Luban.dll
set CLIENT_PATH=..\..\Client\Assets

dotnet %LUBAN_DLL% ^
    -t all ^
    -c cs-simple-json ^
    -d json2 ^
    --conf %WORKSPACE%\luban.conf ^
    -x outputDataDir=%CLIENT_PATH%\GameRes\DataTables ^
    -x outputCodeDir=%CLIENT_PATH%\GameScripts\HotUpdate\DataTables

pause