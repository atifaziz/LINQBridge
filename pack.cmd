@echo off
setlocal
chcp 1252
pushd "%~dp0"
if not exist dist md dist
if not %errorlevel%==0 exit /b %errorlevel%
call :packlib && call :packcs
goto :EOF

:packlib
call build /v:m && call :pack LinqBridge
goto :EOF

:packcs
call onefile > dist\LinqBridge.cs && call :pack LinqBridge.Embedded
goto :EOF

:pack
tools\NuGet pack pkg\%1.nuspec -OutputDirectory dist
goto :EOF
