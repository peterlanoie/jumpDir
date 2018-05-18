@echo off
:: let's abuse the for command to extract the program's output
::echo launch args: %*
for /f "tokens=*" %%i in ('dotnet %~dp0..\src\bin\netcoreapp2.0\Pelasoft.JumpDir.dll %*') do SET JUMPDIRCOMMAND=%%i
::echo received jumpDir command: %JUMPDIRCOMMAND%
%JUMPDIRCOMMAND%
SET JUMPDIRCOMMAND=