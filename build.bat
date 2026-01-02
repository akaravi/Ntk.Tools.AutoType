@echo off
chcp 65001 >nul
echo ========================================
echo   کامپایل برنامه Ntk.Tools.AutoType
echo ========================================
echo.

echo در حال کامپایل...
dotnet build -c Release -o bin/Release

if errorlevel 1 (
    echo خطا در کامپایل!
    pause
    exit /b 1
)

echo.
echo ✓ کامپایل با موفقیت انجام شد!
echo ✓ فایل اجرایی در مسیر bin\Release\Ntk.Tools.AutoType.exe ایجاد شد
echo.
echo برای اجرا:
echo   bin\Release\Ntk.Tools.AutoType.exe "متن" ^<دقیقه^> ^<تعداد اجرا^>
echo   یا
echo   run.bat "متن" ^<دقیقه^> ^<تعداد اجرا^>
echo.
pause

