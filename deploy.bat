@echo off
set MOD_ID=CustomCardBalance
set PROJECT_NAME=CustomCardBalance
if not defined STS2_GAME_DIR set "STS2_GAME_DIR=%ProgramFiles(x86)%\Steam\steamapps\common\Slay the Spire 2"
set "GAME_MODS=%STS2_GAME_DIR%\mods\%MOD_ID%"
set BUILD_DIR=bin\Release\net9.0

echo ===== Compiling =====
dotnet build -c Release --no-restore /p:Sts2GameDir="%STS2_GAME_DIR%"
if %ERRORLEVEL% NEQ 0 (
    echo BUILD FAILED!
    pause
    exit /b 1
)

echo ===== Deploying to game =====
if not exist "%GAME_MODS%" mkdir "%GAME_MODS%"
copy /Y "%BUILD_DIR%\%PROJECT_NAME%.dll" "%GAME_MODS%\"
copy /Y "%PROJECT_NAME%.json" "%GAME_MODS%\"
if not exist "%GAME_MODS%\assets" mkdir "%GAME_MODS%\assets"
copy /Y "%BUILD_DIR%\assets\*.png" "%GAME_MODS%\assets\"

echo ===== Done =====
