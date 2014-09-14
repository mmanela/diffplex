@echo off

if ERRORLEVEL 1 goto end
set Configuration=Debug

if /i "%1"=="CI" goto CI
goto Build

:CI
set Configuration=Release
goto Build



:Build

packages\psake.4.3.2\tools\psake.cmd "default.ps1 %* -properties @{configuration='%Configuration%'} -parameters @{arg0='%1'; arg1='%2'; arg2='%3'} -framework '4.0'"


goto end


:end
IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%