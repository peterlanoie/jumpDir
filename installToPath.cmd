@echo off
where jd.cmd /Q
::echo where check returned %ERRORLEVEL%
IF %ERRORLEVEL%==0 (
	echo `jd.cmd` appears to already be in the path. Nothing to else add.
	goto end
)
echo Can't find `jd` command in current path.
echo.
echo Command search path to add: %~dp0script
echo.
echo Possible path variable locations:
echo   M - Machine/system wide path (persistent)
echo   U - User path (persistent)
echo   C - Current command session only (temporary)
echo.
choice /C cumq /N /M "Which path to add to (or Q to skip)? "
if ERRORLEVEL 3 goto setmachinepath
if ERRORLEVEL 2 goto setuserpath
if ERRORLEVEL 1 goto setlocalpath
goto end

:setlocalpath
echo Setting to current command session only.
SET "PATH=%~dp0script;%PATH%"
goto end

:setuserpath
echo Setting to user environment variable.
::SETX "PATH=%~dp0script;%PATH%"
goto end

:setmachinepath
echo Setting to system wide environment variable.
::SETX /M "PATH=%~dp0script;%PATH%"
goto end

:end
