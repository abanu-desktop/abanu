cd %~dp0
cd ..

tools\fastcopy.exe /auto_close Z:\PanelShell /to="C:\Temp\PanelShell"

rem cd C:\Temp\PanelShell\src\Core\bin\Debug
rem PanelShell.exe

c:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe C:\Temp\PanelShell\src\PanelShell.sln

cd C:\Temp\PanelShell\scripts