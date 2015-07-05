cd %~dp0
cd ..

tools\fastcopy.exe /auto_close Z:\abanu /to="C:\Temp\abanu"

c:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe C:\Temp\abanu\src\abanu.sln

cd C:\Temp\abanu\scripts
