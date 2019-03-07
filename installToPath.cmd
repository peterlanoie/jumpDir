@echo off
where jd.cmd /Q
::echo where check returned %ERRORLEVEL%
IF %ERRORLEVEL%==0 (
	echo `jd.cmd` appears to already be in the path. Nothing else to add.
	goto end
)
echo Can't find `jd` command in current path.
echo.
echo Command search path to add: %~dp0script
echo.
echo Possible path variable locations:
::echo   M - Machine/system wide path (persistent) and current session
::echo       This requires execution as admin. It will fail otherwise.
echo   U - User path (persistent) and current session
echo   S - Current command session only (temporary)
echo.
choice /C suq /N /M "Which path to add to (or Q to skip)? "
if ERRORLEVEL 3 goto end
::if ERRORLEVEL 3 goto setmachinepath
if ERRORLEVEL 2 goto setuserpath
if ERRORLEVEL 1 goto setlocalpath
goto end

:setuserpath
echo Setting to user environment variable.
SETX PATH "%~dp0script;%PATH%"
goto setlocalpath

:setmachinepath
::echo !!! Not implemented yet.
SET tmp_RegKey="HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment"
FOR /F "usebackq tokens=2*" %%A IN (`REG QUERY %tmp_RegKey% /v PATH`) DO Set tmp_CurrPath=%%B
ECHO %tmp_CurrPath% > system_path_bak.txt
ECHO Current system path saved to `system_path_bak.txt`.
echo Setting to system wide environment variable.
SETX PATH "%CurrPath%";%1 /M
SET tmp_RegKey=
SET tmp_CurrPath=

:setlocalpath
echo Setting to current command session.
SET "PATH=%~dp0script;%PATH%"
goto end

:end
