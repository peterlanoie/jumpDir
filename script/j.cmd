@echo off
for /f "tokens=*" %%i in ('dotnet D:\Pelasoft\Pelasoft.JumpDir\bin\Debug\netcoreapp2.0\Pelasoft.JumpDir.dll %*') do set JUMPDIRRESULT=%%i
::echo %JUMPDIRRESULT%
if not "%JUMPDIRRESULT%" == "[no target]" cd %JUMPDIRRESULT%
::dotnet D:\Pelasoft\Pelasoft.JumpDir\bin\Debug\netcoreapp2.0\Pelasoft.JumpDir.dll %*