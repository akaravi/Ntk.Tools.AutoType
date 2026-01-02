# Ntk.Tools.AutoType

<div dir="rtl">

## 📝 درباره پروژه

**Ntk.Tools.AutoType** یک ابزار قدرتمند و حرفه‌ای برای تایپ خودکار متن در Windows است که با استفاده از .NET 8.0 و Windows Forms ساخته شده است. این برنامه به شما امکان می‌دهد متن مورد نظر خود را به صورت خودکار و در بازه‌های زمانی مشخص تایپ کند.

[![.NET Version](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Version](https://img.shields.io/badge/Version-1.1.2-orange.svg)](https://github.com/yourusername/Ntk.Tools.AutoType/releases)

</div>

---

## ✨ ویژگی‌ها

### ویژگی‌های اصلی

- ✅ **تایپ خودکار**: تایپ خودکار متن در محل قرارگیری کرسر با استفاده از Clipboard
- ⏰ **فاصله زمانی قابل تنظیم**: تنظیم فاصله زمانی بین هر تایپ (به دقیقه)
- 🔄 **تعداد اجرای قابل تنظیم**: محدود کردن تعداد اجرا یا اجرای نامحدود
- ⏸️ **توقف موقت و ادامه**: امکان pause/resume برنامه با کلید P یا Space
- 🔔 **System Tray Integration**: اجرا در System Tray (کنار ساعت ویندوز)
- 📢 **Toast Notifications**: نمایش پاپ‌آپ با اطلاعات اجرای بعدی
- 🌐 **پشتیبانی از Unicode**: پشتیبانی کامل از کاراکترهای فارسی، انگلیسی و سایر زبان‌ها
- 📄 **خواندن از فایل**: امکان خواندن متن از فایل متنی با Encoding UTF-8
- ⌨️ **فشردن خودکار Enter**: پس از هر تایپ، کلید Enter به صورت خودکار فشرده می‌شود
- 🎯 **رابط خط فرمان ساده**: استفاده آسان از طریق خط فرمان یا حالت تعاملی
- 🚀 **Async/Await**: استفاده کامل از async/await برای عملکرد بهتر
- 📦 **Single File Build**: خروجی به صورت یک فایل واحد (Self-contained یا Framework-dependent)

---

## 🏗️ معماری و ساختار

### معماری کلی

این برنامه از یک معماری async و event-driven استفاده می‌کند:

```
┌─────────────────────────────────────────────────┐
│         Program.cs (Main Entry - Async)          │
│  - دریافت پارامترها از خط فرمان                │
│  - مدیریت async tasks                           │
│  - System Tray Integration                       │
└──────────────┬───────────────────────────────────┘
               │
               ├─────────────────┬──────────────────┐
               ▼                 ▼                  ▼
┌──────────────────────┐  ┌──────────────┐  ┌──────────────┐
│  MainLoopAsync()     │  │ Keyboard     │  │ Tooltip      │
│  - حلقه اصلی اجرا    │  │ Listener     │  │ Updater      │
│  - مدیریت pause      │  │ - P/Space   │  │ - هر 5 ثانیه │
└──────────┬───────────┘  └──────────────┘  └──────────────┘
           │
           ▼
┌─────────────────────────────────────────┐
│         TypeTextAsync() Method           │
│  - استفاده از Clipboard برای تایپ       │
│  - Windows Forms Clipboard (اولویت)     │
│  - Fallback به Native API                │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│    Windows API (P/Invoke)               │
│  - user32.dll (Clipboard, Keyboard)    │
│  - kernel32.dll (Memory Management)     │
└─────────────────────────────────────────┘
```

### ساختار کد

#### 1. **P/Invoke Declarations** (خطوط 12-37)
```csharp
[DllImport("user32.dll")]
static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
```
- استفاده از Windows API برای دسترسی به کیبورد و Clipboard
- تعریف توابع Native برای مدیریت حافظه (GlobalAlloc, GlobalLock, etc.)

#### 2. **Main Method (Async)** (خطوط 53-200)
- نقطه ورود async برنامه
- پردازش پارامترهای خط فرمان
- راه‌اندازی System Tray در thread جداگانه
- مدیریت async tasks (KeyboardListener, TooltipUpdater)
- مدیریت CancellationToken برای توقف صحیح

#### 3. **System Tray Integration** (خطوط 249-397)
- `InitializeSystemTray()`: ایجاد NotifyIcon و Context Menu
- `CreateApplicationIcon()`: ایجاد آیکون سفارشی با "AT"
- `ShowNotification()`: نمایش Toast Notifications
- `UpdateTooltipAsync()`: به‌روزرسانی خودکار tooltip

#### 4. **TypeTextAsync Method** (خطوط 449-486)
- استفاده از Clipboard برای تایپ متن
- استفاده از Windows Forms Clipboard (اولویت اول)
- Fallback به Native API در صورت خطا
- استفاده از Ctrl+V برای Paste کردن متن
- مدیریت async delays

#### 5. **SetClipboardTextAsync Method** (خطوط 488-555)
- پیاده‌سازی Native Clipboard با استفاده از Windows API
- مدیریت حافظه و Lock/Unlock
- Retry Logic برای اطمینان از موفقیت
- پشتیبانی از Unicode

#### 6. **Pause/Resume System** (خطوط 657-703)
- `TogglePauseAsync()`: تغییر وضعیت pause/resume
- استفاده از `ManualResetEventSlim` برای هماهنگی
- نمایش notifications برای pause/resume
- محاسبه مجدد زمان اجرای بعدی

#### 7. **Keyboard Listener** (خطوط 616-655)
- `KeyboardListenerAsync()`: گوش دادن به کلیدها در پس‌زمینه
- تشخیص کلید P یا Space برای pause/resume
- اجرا در async task جداگانه

### تکنولوژی‌های استفاده شده

- **.NET 8.0**: Framework اصلی با async/await
- **Windows Forms**: برای دسترسی به Clipboard و System Tray
- **P/Invoke**: برای دسترسی به Windows API (user32.dll, kernel32.dll)
- **Unicode Encoding**: برای پشتیبانی از کاراکترهای چندبایتی
- **Async/Await**: برای عملکرد بهتر و non-blocking operations
- **CancellationToken**: برای مدیریت توقف برنامه
- **ManualResetEventSlim**: برای هماهنگی pause/resume

---

## 📦 نصب و راه‌اندازی

### پیش‌نیازها

- **Windows 10/11** یا Windows Server 2016+
- **.NET 8.0 Runtime** (فقط برای Framework-dependent version)
- **.NET 8.0 SDK** (برای کامپایل از source)

### نصب از Release (پیشنهادی)

1. به بخش [Releases](https://github.com/yourusername/Ntk.Tools.AutoType/releases) بروید
2. نسخه مناسب را دانلود کنید:

   **Self-contained** (پیشنهادی برای اکثر کاربران):
   - `win-x64-self-contained.exe` (~68 MB) - نیازی به نصب .NET ندارد
   - `win-x86-self-contained.exe` (~63 MB) - نیازی به نصب .NET ندارد

   **Framework-dependent** (برای کاربرانی که .NET 8.0 نصب دارند):
   - `win-x64-framework-dependent.exe` (~0.18 MB) - نیاز به .NET 8.0 Runtime
   - `win-x86-framework-dependent.exe` (~0.18 MB) - نیاز به .NET 8.0 Runtime

3. فایل `.exe` را دانلود و اجرا کنید

### نصب از Source

```bash
# Clone repository
git clone https://github.com/yourusername/Ntk.Tools.AutoType.git
cd Ntk.Tools.AutoType

# Restore dependencies
dotnet restore

# Build project
dotnet build -c Release

# یا استفاده از build.bat
build.bat
```

---

## 🚀 استفاده

### روش 1: حالت تعاملی (پیشنهادی)

```bash
# اجرای برنامه بدون پارامتر
Ntk.Tools.AutoType.exe
```

برنامه از شما سه مقدار را خط به خط می‌پرسد:
1. **متن مورد نظر** برای تایپ (یا نام فایل مثل `message.txt`)
2. **فاصله زمانی** به دقیقه
3. **تعداد اجرا** (0 برای نامحدود)

**مثال:**
```
=== Ntk.Tools.AutoType - Auto Type Program ===

Enter text to type (or filename like my.txt): سلام دنیا
Enter time interval in minutes: 5
Enter number of executions (0 for unlimited): 10

Auto Type program started...
Text: سلام دنیا
Time interval: 5 minutes
Max executions: 10
Press Ctrl+C to stop the program
Press P or Space to pause/resume the program

Starting in 5 seconds...
```

### روش 2: استفاده از پارامترهای خط فرمان

```bash
Ntk.Tools.AutoType.exe <text|filename> <minutes> <maxExecutions>
```

**پارامترها:**
- `text|filename`: متن مورد نظر یا نام فایل (مثل `message.txt`)
- `minutes`: فاصله زمانی به دقیقه (باید عدد مثبت باشد)
- `maxExecutions`: تعداد دفعات اجرا (0 برای اجرای نامحدود)

**مثال‌ها:**

```bash
# تایپ "سلام دنیا" هر 5 دقیقه، 10 بار
Ntk.Tools.AutoType.exe "سلام دنیا" 5 10

# تایپ "ادامه بده" هر 2 دقیقه، نامحدود
Ntk.Tools.AutoType.exe "ادامه بده" 2 0

# استفاده از فایل
Ntk.Tools.AutoType.exe message.txt 3 5
```

### استفاده از فایل متنی

می‌توانید متن را در یک فایل متنی ذخیره کنید و نام فایل را به عنوان پارامتر اول وارد کنید:

```bash
# ایجاد فایل
echo "متن مورد نظر" > message.txt

# استفاده از فایل
Ntk.Tools.AutoType.exe message.txt 5 10
```

**نکات:**
- فایل باید در همان دایرکتوری برنامه یا مسیر کامل باشد
- فایل باید با Encoding UTF-8 ذخیره شود
- برنامه به صورت خودکار تشخیص می‌دهد که ورودی یک فایل است یا متن مستقیم

---

## 🎮 کنترل برنامه

### کلیدهای میانبر

- **P یا Space**: توقف موقت (Pause) / ادامه (Resume) برنامه
- **Ctrl+C**: توقف کامل برنامه

### System Tray

برنامه در System Tray (کنار ساعت ویندوز) اجرا می‌شود:

- **کلیک چپ روی آیکون**: نمایش پاپ‌آپ با اطلاعات برنامه
- **کلیک راست روی آیکون**: نمایش منوی زمینه
  - **Pause/Resume**: توقف موقت یا ادامه
  - **Show Info**: نمایش اطلاعات در پاپ‌آپ
  - **Exit**: خروج از برنامه

### Toast Notifications

برنامه به صورت خودکار پاپ‌آپ نمایش می‌دهد:
- هنگام شروع برنامه
- پس از هر اجرا (با زمان اجرای بعدی)
- هنگام pause/resume

---

## 📋 مثال‌های کاربردی

### مثال 1: تایپ پیام در چت

```bash
# تایپ "ادامه بده" هر 2 دقیقه، نامحدود
Ntk.Tools.AutoType.exe "ادامه بده" 2 0
```

### مثال 2: تایپ متن طولانی از فایل

```bash
# محتوای فایل message.txt
# این یک متن طولانی است که می‌خواهم به صورت خودکار تایپ شود.

# اجرا
Ntk.Tools.AutoType.exe message.txt 10 5
```

### مثال 3: تایپ با تعداد محدود

```bash
# تایپ "تست" هر 1 دقیقه، 20 بار
Ntk.Tools.AutoType.exe "تست" 1 20
```

---

## 🛠️ اسکریپت‌های کمکی

پروژه شامل چند اسکریپت Batch برای سهولت استفاده است:

### `build.bat`
کامپایل برنامه در حالت Release و ایجاد Single File:
```bash
build.bat
```
خروجی: `bin\Release\Ntk.Tools.AutoType.exe`

### `run.bat`
کامپایل و اجرای سریع برنامه:
```bash
# حالت تعاملی
run.bat

# با پارامترها
run.bat "متن" 5 10
```

### `stop.bat`
توقف تمام نمونه‌های در حال اجرای برنامه:
```bash
stop.bat
```

---

## ⚙️ تنظیمات و نکات مهم

### نکات استفاده

1. **قرار دادن کرسر**: قبل از اجرای برنامه، کرسر را در محل مورد نظر (مثل فیلد ورودی چت) قرار دهید
2. **زمان آماده‌سازی**: برنامه 5 ثانیه پس از اجرا شروع به تایپ می‌کند
3. **System Tray**: برنامه در System Tray اجرا می‌شود و می‌توانید از آنجا مدیریت کنید
4. **Pause/Resume**: می‌توانید برنامه را موقتاً متوقف کنید و دوباره ادامه دهید
5. **Notifications**: پاپ‌آپ‌ها اطلاعات اجرای بعدی را نمایش می‌دهند

### محدودیت‌ها

- فقط در Windows کار می‌کند
- نیاز به دسترسی به Clipboard دارد
- نیاز به دسترسی به کیبورد دارد
- نیاز به دسترسی به محیط گرافیکی Windows دارد

---

## 🔧 توسعه

### ساختار پروژه

```
Ntk.Tools.AutoType/
├── .github/
│   └── workflows/
│       └── build-and-release.yml    # CI/CD Pipeline (GitHub Actions)
├── Program.cs                        # کد اصلی برنامه (Async)
├── Ntk.Tools.AutoType.csproj        # فایل پروژه
├── Ntk.Tools.AutoType.sln           # Solution File
├── README.md                         # این فایل
├── .gitignore                        # Git Ignore Rules
├── build.bat                         # اسکریپت Build
├── run.bat                           # اسکریپت Run
└── stop.bat                          # اسکریپت Stop
```

### کامپایل از Source

```bash
# Debug
dotnet build

# Release (Framework-dependent - کوچک)
dotnet publish -c Release -o bin/Release

# Release (Self-contained - بزرگ اما مستقل)
dotnet publish -c Release -r win-x64 --self-contained true -o bin/Release
```

### Build Configurations

#### Framework-Dependent (پیش‌فرض)
- حجم: ~0.18 MB
- نیاز به .NET 8.0 Runtime
- دستور: `dotnet publish -c Release -o output`

#### Self-Contained
- حجم: ~68 MB (x64) یا ~63 MB (x86)
- مستقل (نیازی به .NET ندارد)
- دستور: `dotnet publish -c Release -r win-x64 --self-contained true -o output`

---

## 🤝 مشارکت

مشارکت‌ها، Issues و Pull Request‌ها خوش‌آمد هستند! برای تغییرات بزرگ، لطفاً ابتدا یک Issue باز کنید تا بتوانیم در مورد تغییرات مورد نظر بحث کنیم.

### راهنمای مشارکت

1. Fork کنید
2. یک Branch برای ویژگی خود ایجاد کنید (`git checkout -b feature/AmazingFeature`)
3. تغییرات خود را Commit کنید (`git commit -m 'Add some AmazingFeature'`)
4. به Branch خود Push کنید (`git push origin feature/AmazingFeature`)
5. یک Pull Request باز کنید

---

## 📄 لایسنس

این پروژه تحت لایسنس MIT منتشر شده است. برای جزئیات بیشتر، فایل `LICENSE` را ببینید.

---

## 📞 تماس و پشتیبانی

- **Issues**: برای گزارش باگ یا درخواست ویژگی، از [GitHub Issues](https://github.com/yourusername/Ntk.Tools.AutoType/issues) استفاده کنید

---

## 🎯 Roadmap

- [x] System Tray Integration
- [x] Toast Notifications
- [x] Pause/Resume functionality
- [x] Async/Await implementation
- [x] Custom application icon
- [x] Single File builds
- [ ] افزودن GUI برای تنظیمات
- [ ] افزودن Logging به فایل
- [ ] افزودن پشتیبانی از چندین متن
- [ ] افزودن پشتیبانی از Scheduled Tasks
- [ ] افزودن Hotkeys برای کنترل

---

## 📊 نسخه‌ها

### نسخه 1.1.2 (جاری)
- بهبود System Tray
- بهینه‌سازی حجم فایل‌های framework-dependent
- بهبود مدیریت async tasks

### نسخه 1.1.0
- اضافه شدن System Tray
- اضافه شدن Toast Notifications
- اضافه شدن Pause/Resume
- تبدیل کامل به async/await

### نسخه 1.0.0
- نسخه اولیه
- تایپ خودکار متن
- پشتیبانی از فایل
- پشتیبانی از Unicode

---

<div align="center">

**ساخته شده با ❤️ توسط NTK**

[⭐ Star این پروژه را بدهید](https://github.com/yourusername/Ntk.Tools.AutoType)

</div>
