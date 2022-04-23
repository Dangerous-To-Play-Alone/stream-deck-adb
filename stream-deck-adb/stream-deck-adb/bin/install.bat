setlocal
cd /d %~dp0
cd %1

REM *** MAKE SURE THE FOLLOWING VARIABLES ARE CORRECT ***
REM (Distribution tool be downloaded from: https://developer.elgato.com/documentation/stream-deck/sdk/exporting-your-plugin/ )
SET OUTPUT_DIR="C:\TEMP"
SET DISTRIBUTION_TOOL="C:\Projects\StreamDeck\DistributionTool.exe"
SET STREAM_DECK_FILE="C:\Program Files\Elgato\StreamDeck\StreamDeck.exe"
SET STREAM_DECK_LOAD_TIMEOUT=15

taskkill /f /im streamdeck.exe
taskkill /f /im stream-deck-adb.exe
timeout /t 2
del %OUTPUT_DIR%\%2.streamDeckPlugin
%DISTRIBUTION_TOOL% -b -i %2.sdPlugin -o %OUTPUT_DIR%
rmdir %APPDATA%\Elgato\StreamDeck\Plugins\%2.sdPlugin /s /q

Xcopy /E /I %2.sdPlugin %APPDATA%\Elgato\StreamDeck\Plugins\%2.sdPlugin

START "" %STREAM_DECK_FILE%
timeout /t %STREAM_DECK_LOAD_TIMEOUT%
REM %OUTPUT_DIR%\%2.streamDeckPlugin

cd ../
