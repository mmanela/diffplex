@echo off

REM build 
REM build build [skiptests]

SET TARGET="build"

IF /I "%1"=="skiptests" (
	set SKIPTESTS="1"
	SHIFT
)

IF NOT [%1]==[] (set TARGET="%1")

"packages\FAKE.4.45.2\tools\Fake.exe" "Build\\Targets.fsx" "target=%TARGET%" "skiptests=%SKIPTESTS%"
