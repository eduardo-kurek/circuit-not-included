@echo off
setlocal

rem Verifying if the dotenv.env file exists
if not exist .env (
    echo .env file not found.
    pause
    exit /b 1
)

rem Loading environment variables declared in dotenv.env
for /F "usebackq tokens=1,* delims==" %%A in (".env") do (
    set "%%A=%%B"
)

set SOURCE_DLL=bin\Release\%ASSEMBLY_NAME%
set PASTE_FOLDER=%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Dev\%DEV_FOLDER_NAME%
set LOG_PATH=%USERPROFILE%\AppData\LocalLow\Klei\Oxygen Not Included\Player.log

rem Creating mod-folder and copying the files
echo Copying files to %PASTE_FOLDER%
rmdir /s /q %PASTE_FOLDER%
xcopy /i /s /e /y "mod-folder" %PASTE_FOLDER%
copy /y %SOURCE_DLL% %PASTE_FOLDER%

rem Starting the game
start steam://rungameid/%ONI_STEAMGAME_ID% /wait

timeout /nobreak /t 10 >nul

rem Verifying if the game is running
:loop
tasklist /FI "IMAGENAME eq OxygenNotIncluded.exe" | find "OxygenNotIncluded.exe" >nul
if %ERRORLEVEL% equ 0 (
    timeout /nobreak /t 2 >nul
    goto loop
) else (
    start notepad %LOG_PATH%
)

endlocal