@echo off
setlocal

rem Verifying if the dotenv.env file exists
if not exist .env (
    echo .env file not found.
    exit
)

rem Loading environment variables declared in dotenv.env
for /F "usebackq tokens=1,* delims==" %%A in (".env") do (
    set "%%A=%%B"
)

rem Copying the libraries from the game to the project
mkdir lib > nul 2>&1
if not exist "lib\0Harmony.dll" copy "%ONI_MANAGED_FOLDER%\0Harmony.dll" "lib\0Harmony.dll"
if not exist "lib\Assembly-CSharp.dll" copy "%ONI_MANAGED_FOLDER%\Assembly-CSharp.dll" "lib\Assembly-CSharp.dll"
if not exist "lib\Assembly-CSharp-firstpass.dll" copy "%ONI_MANAGED_FOLDER%\Assembly-CSharp-firstpass.dll" "lib\Assembly-CSharp-firstpass.dll"
if not exist "lib\UnityEngine.dll" copy "%ONI_MANAGED_FOLDER%\UnityEngine.dll" "lib\UnityEngine.dll"
if not exist "lib\UnityEngine.CoreModule.dll" copy "%ONI_MANAGED_FOLDER%\UnityEngine.CoreModule.dll" "lib\UnityEngine.CoreModule.dll"
if not exist "lib\UnityEngine.UI.dll" copy "%ONI_MANAGED_FOLDER%\UnityEngine.UI.dll" "lib\UnityEngine.UI.dll"

endlocal