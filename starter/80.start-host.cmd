@echo off
cls
title start-all
set stime=30
for /f "delims=" %%i in ('dir *.bat /b') do (
    echo %%i
    start %%i
    ping -n %stime% 127.1 >nul
)