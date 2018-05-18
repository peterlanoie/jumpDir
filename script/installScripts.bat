@echo off
echo == jumpDir script installer script. ==
echo.
if "%1"=="" (
	echo.
	echo !! Missing argument !!
	echo Please provide an install folder path for the shortcut scripts.
	echo This should be a folder that's already accessible from your system path.
	goto end
)

echo Installing shortcut scripts into '%1'...
echo @%~dp0%jumpDir.cmd %%*

::echo %~dp0%
::dir "%~dp0%"

echo Done!
echo.
:end