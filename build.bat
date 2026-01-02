@echo off
chcp 65001 >nul
echo ========================================
echo   کامپایل برنامه Ntk.Tools.AutoType
echo ========================================
echo.

echo در حال کامپایل و ایجاد فایل واحد...
dotnet publish Ntk.Tools.AutoType.csproj -c Release -o bin/Release

if errorlevel 1 (
    echo خطا در کامپایل!
    pause
    exit /b 1
)

echo.
echo در حال پاکسازی فایل‌های اضافی...
if exist "bin\Release\*.dll" del /q "bin\Release\*.dll" 2>nul
if exist "bin\Release\*.pdb" del /q "bin\Release\*.pdb" 2>nul
if exist "bin\Release\*.deps.json" del /q "bin\Release\*.deps.json" 2>nul
if exist "bin\Release\*.runtimeconfig.json" del /q "bin\Release\*.runtimeconfig.json" 2>nul
if exist "bin\Release\net8.0-windows" rmdir /s /q "bin\Release\net8.0-windows" 2>nul

echo.
echo ✓ کامپایل با موفقیت انجام شد!
echo ✓ فایل اجرایی واحد در مسیر bin\Release\Ntk.Tools.AutoType.exe ایجاد شد
echo.
echo برای اجرا:
echo   bin\Release\Ntk.Tools.AutoType.exe "متن" ^<دقیقه^> ^<تعداد اجرا^>
echo   یا
echo   run.bat "متن" ^<دقیقه^> ^<تعداد اجرا^>
echo.
pause

