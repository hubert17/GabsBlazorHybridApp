@echo off
title Cloudflared Access Setup Utility

:: Define the explicit path to the cloudflared executable installed by Chocolatey
set "CLOUDFLARED_EXE=C:\ProgramData\chocolatey\lib\cloudflared\tools\cloudflared.exe"

:: --- Welcome Message & Description ---
echo.
echo #######################################################
echo #       Welcome to the Cloudflared Access Setup       #
echo #######################################################
echo.
echo Cloudflare Access lets you securely reach private services
echo and internal resources (like databases or servers) 
echo behind Cloudflare without exposing them to the public Internet.
echo.

:: --- Prompt for Cloudflared Installation ---
:INSTALL_PROMPT
echo.
set /p install_cf="Do you want to install Cloudflared via Chocolatey (Y/n)? (Press Enter to Skip/No): "
if /i "%install_cf%"=="Y" (
    echo.
    echo Installing Cloudflared via Chocolatey...
    choco install cloudflared --force -y
    if errorlevel 1 (
        echo ERROR: Installation failed or Chocolatey is not installed.
        echo Try running from an elevated command shell access.
        pause
        goto END
    )
    echo Refreshing environment variables...
    call refreshenv
) else if /i "%install_cf%"=="n" (
    echo Skipping Cloudflared installation.
) else if "%install_cf%"=="" (
    echo Skipping Cloudflared installation.
) else (
    echo Invalid input. Please enter 'Y', 'n', or press Enter.
    goto INSTALL_PROMPT
)

:: --- Check for cloudflared.exe existence before proceeding ---
if not exist "%CLOUDFLARED_EXE%" (
    echo.
    echo FATAL ERROR: The cloudflared executable was not found at the expected location:
    echo "%CLOUDFLARED_EXE%"
    echo Please ensure Chocolatey finished its install successfully.
    pause
    goto END
)

:: --- Prompt for Hostname ---
echo.
set /p target_hostname="Enter the target hostname (e.g., mytcp.yourdomain.com): "
if "%target_hostname%"=="" (
    echo ERROR: Hostname cannot be empty.
    pause
    goto END
)

:: --- Prompt for Listener Port ---
echo.
set /p listener_port="Enter the local listener port (e.g., 11433): "
if "%listener_port%"=="" (
    echo ERROR: Port cannot be empty.
    pause
    goto END
)

:: --- Execute Cloudflared Command using the full path ---
echo.
echo Starting Cloudflared Access Tunnel...
echo Command: "%CLOUDFLARED_EXE%" access tcp --hostname %target_hostname% --listener 127.0.0.1:%listener_port%
echo.

:: Execute the tunnel command using the explicit path
"%CLOUDFLARED_EXE%" access tcp --hostname %target_hostname% --listener 127.0.0.1:%listener_port%

:END
echo.
echo Tunnel session ended or failed.
pause