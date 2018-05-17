@echo off
for /f "tokens=*" %%i in ('dotnet %~dp0..\src\bin\Debug\netcoreapp2.0\Pelasoft.JumpDir.dll %*') do set JUMPDIRRESULT=%%i
if not "%JUMPDIRRESULT%" == "[no target]" cd %JUMPDIRRESULT%