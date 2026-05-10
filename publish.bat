@echo off
setlocal EnableExtensions
cd /d "%~dp0"

rem ---------------------------------------------------------------------------
rem Pixel Planets — Windows publish (Release, win-x64) into .\published
rem
rem Footprint notes (MonoGame / WinForms / WindowsDX):
rem   - Native AOT is not a realistic option: MonoGame content, WinForms, and
rem     DirectX interop depend on reflection and runtime loading; expect crashes
rem     or unsupported scenarios if you force PublishAot.
rem   - Do not enable PublishTrimmed unless you invest in trim roots; it often
rem     breaks MonoGame content readers and shader/effect loading.
rem   - Smallest *folder* for players who already have .NET 8 Desktop Runtime:
rem       set SELFCONTAINED=0
rem     (~17 MB single-file exe in tests, plus Content\*.xnb — no bundled runtime)
rem   - Smallest *portable* zip (no separate runtime install):
rem       set SELFCONTAINED=1   (default)
rem     (single compressed self-contained exe + Content; larger but self-contained)
rem ---------------------------------------------------------------------------

set "OUT=published"
set "RID=win-x64"
set "CFG=Release"

rem 1 = self-contained portable build (default). 0 = framework-dependent (needs .NET 8 Desktop on the PC).
if not defined SELFCONTAINED set "SELFCONTAINED=1"

if exist "%OUT%" rmdir /s /q "%OUT%"
if errorlevel 1 (
  echo Failed to remove existing "%OUT%". Close apps using files there and retry.
  exit /b 1
)

if "%SELFCONTAINED%"=="1" goto :publish_selfcontained
if "%SELFCONTAINED%"=="0" goto :publish_frameworkdep
echo SELFCONTAINED must be 0 or 1 (got "%SELFCONTAINED%").
exit /b 1

:publish_selfcontained
dotnet publish PixelPlanets.csproj -c "%CFG%" -r "%RID%" --self-contained true -o "%OUT%" ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:EnableCompressionInSingleFile=true ^
  -p:DebugType=none ^
  -p:DebugSymbols=false
goto :after_publish

:publish_frameworkdep
rem EnableCompressionInSingleFile requires self-contained (NETSDK1176).
dotnet publish PixelPlanets.csproj -c "%CFG%" -r "%RID%" --self-contained false -o "%OUT%" ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:DebugType=none ^
  -p:DebugSymbols=false

:after_publish
if errorlevel 1 exit /b 1

echo.
echo Done. Output: "%~dp0%OUT%"
echo.
exit /b 0
