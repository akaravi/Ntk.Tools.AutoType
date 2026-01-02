# Ntk.Tools.AutoType

<div dir="rtl">

## 📝 درباره پروژه

**Ntk.Tools.AutoType** یک ابزار قدرتمند و ساده برای تایپ خودکار متن در Windows است که با استفاده از .NET 8.0 و Windows Forms ساخته شده است. این برنامه به شما امکان می‌دهد متن مورد نظر خود را به صورت خودکار و در بازه‌های زمانی مشخص تایپ کند.

[![.NET Version](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

</div>

---

## ✨ ویژگی‌ها

- ✅ **تایپ خودکار**: تایپ خودکار متن در محل قرارگیری کرسر
- ⏰ **فاصله زمانی قابل تنظیم**: تنظیم فاصله زمانی بین هر تایپ (به دقیقه)
- 🔄 **تعداد اجرای قابل تنظیم**: محدود کردن تعداد اجرا یا اجرای نامحدود
- 🌐 **پشتیبانی از Unicode**: پشتیبانی کامل از کاراکترهای فارسی، انگلیسی و سایر زبان‌ها
- 📄 **خواندن از فایل**: امکان خواندن متن از فایل متنی
- ⌨️ **فشردن خودکار Enter**: پس از هر تایپ، کلید Enter به صورت خودکار فشرده می‌شود
- 🎯 **رابط خط فرمان ساده**: استفاده آسان از طریق خط فرمان یا حالت تعاملی

---

## 🏗️ معماری و ساختار

### معماری کلی

این برنامه از یک معماری ساده و کارآمد استفاده می‌کند:

```
┌─────────────────────────────────────────┐
│         Program.cs (Main Entry)         │
│  - دریافت پارامترها از خط فرمان        │
│  - مدیریت حلقه اصلی اجرا               │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│         TypeText() Method                │
│  - استفاده از Clipboard برای تایپ       │
│  - پشتیبانی از Windows Forms Clipboard  │
│  - Fallback به Native API                │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│    Windows API (P/Invoke)               │
│  - user32.dll (Clipboard, Keyboard)     │
│  - kernel32.dll (Memory Management)     │
└─────────────────────────────────────────┘
```

### ساختار کد

#### 1. **P/Invoke Declarations** (خطوط 9-34)
```csharp
[DllImport("user32.dll")]
static extern void keybd_event(...);
```
- استفاده از Windows API برای دسترسی به کیبورد و Clipboard
- تعریف توابع Native برای مدیریت حافظه

#### 2. **Main Method** (خطوط 46-167)
- نقطه ورود برنامه
- پردازش پارامترهای خط فرمان
- مدیریت حلقه اصلی اجرا
- مدیریت استثناها و توقف برنامه

#### 3. **TypeText Method** (خطوط 169-202)
- استفاده از Clipboard برای تایپ متن
- استفاده از Windows Forms Clipboard (اولویت اول)
- Fallback به Native API در صورت خطا
- استفاده از Ctrl+V برای Paste کردن متن

#### 4. **SetClipboardText Method** (خطوط 204-271)
- پیاده‌سازی Native Clipboard با استفاده از Windows API
- مدیریت حافظه و Lock/Unlock
- Retry Logic برای اطمینان از موفقیت

#### 5. **LoadTextFromInput Method** (خطوط 273-320)
- تشخیص اینکه ورودی یک فایل است یا متن مستقیم
- خواندن فایل با Encoding UTF-8
- مدیریت خطاها و Fallback

### تکنولوژی‌های استفاده شده

- **.NET 8.0**: Framework اصلی
- **Windows Forms**: برای دسترسی به Clipboard
- **P/Invoke**: برای دسترسی به Windows API
- **Unicode Encoding**: برای پشتیبانی از کاراکترهای چندبایتی

---

## 📦 نصب و راه‌اندازی

### پیش‌نیازها

- **.NET 8.0 SDK** یا بالاتر
- **Windows 10/11** یا Windows Server 2016+
- **Visual Studio 2022** (اختیاری - برای توسعه)

### نصب از Source

```bash
# Clone repository
git clone https://github.com/yourusername/Ntk.Tools.AutoType.git
cd Ntk.Tools.AutoType

# Restore dependencies
dotnet restore

# Build project
dotnet build -c Release
```

### نصب از Release

1. به بخش [Releases](https://github.com/yourusername/Ntk.Tools.AutoType/releases) بروید
2. نسخه مناسب را دانلود کنید:
   - **Self-contained**: نیازی به نصب .NET ندارد (حجم بیشتر)
   - **Framework-dependent**: نیاز به نصب .NET 8.0 Runtime دارد (حجم کمتر)
3. فایل ZIP را Extract کنید
4. فایل اجرایی را اجرا کنید

---

## 🚀 استفاده

### روش 1: حالت تعاملی (پیشنهادی)

```bash
# اجرای برنامه بدون پارامتر
ntk.autoType.exe
```

برنامه از شما سه مقدار را خط به خط می‌پرسد:
1. **متن مورد نظر** برای تایپ (یا نام فایل)
2. **فاصله زمانی** به دقیقه
3. **تعداد اجرا** (0 برای نامحدود)

**مثال:**
```
Enter text to type (or filename like my.txt): سلام دنیا
Enter time interval in minutes: 5
Enter number of executions (0 for unlimited): 10
```

### روش 2: استفاده از پارامترهای خط فرمان

```bash
ntk.autoType.exe <text|filename> <minutes> <maxExecutions>
```

**پارامترها:**
- `text|filename`: متن مورد نظر یا نام فایل (مثل `message.txt`)
- `minutes`: فاصله زمانی به دقیقه (باید عدد مثبت باشد)
- `maxExecutions`: تعداد دفعات اجرا (0 برای اجرای نامحدود)

**مثال‌ها:**

```bash
# تایپ "سلام دنیا" هر 5 دقیقه، 10 بار
ntk.autoType.exe "سلام دنیا" 5 10

# تایپ "ادامه بده" هر 2 دقیقه، نامحدود
ntk.autoType.exe "ادامه بده" 2 0

# استفاده از فایل
ntk.autoType.exe message.txt 3 5
```

### استفاده از فایل متنی

می‌توانید متن را در یک فایل متنی ذخیره کنید و نام فایل را به عنوان پارامتر اول وارد کنید:

```bash
# ایجاد فایل
echo "متن مورد نظر" > message.txt

# استفاده از فایل
ntk.autoType.exe message.txt 5 10
```

**نکات:**
- فایل باید در همان دایرکتوری برنامه یا مسیر کامل باشد
- فایل باید با Encoding UTF-8 ذخیره شود
- برنامه به صورت خودکار تشخیص می‌دهد که ورودی یک فایل است یا متن مستقیم

---

## 📋 مثال‌های کاربردی

### مثال 1: تایپ پیام در چت

```bash
# تایپ "ادامه بده" هر 2 دقیقه، نامحدود
ntk.autoType.exe "ادامه بده" 2 0
```

### مثال 2: تایپ متن طولانی از فایل

```bash
# محتوای فایل message.txt
# این یک متن طولانی است که می‌خواهم به صورت خودکار تایپ شود.

# اجرا
ntk.autoType.exe message.txt 10 5
```

### مثال 3: تایپ با تعداد محدود

```bash
# تایپ "تست" هر 1 دقیقه، 20 بار
ntk.autoType.exe "تست" 1 20
```

---

## 🛠️ اسکریپت‌های کمکی

پروژه شامل چند اسکریپت Batch برای سهولت استفاده است:

### `build.bat`
کامپایل برنامه در حالت Release:
```bash
build.bat
```

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

1. **قرار دادن کرسر**: قبل از اجرای برنامه، کرسر را در محل مورد نظر (مثل فیلد ورودی) قرار دهید
2. **زمان آماده‌سازی**: برنامه 5 ثانیه پس از اجرا شروع به تایپ می‌کند
3. **توقف برنامه**: برای توقف از `Ctrl+C` استفاده کنید
4. **دسترسی به محیط گرافیکی**: برنامه نیاز به دسترسی به محیط گرافیکی Windows دارد

### محدودیت‌ها

- فقط در Windows کار می‌کند
- نیاز به دسترسی به Clipboard دارد
- نیاز به دسترسی به کیبورد دارد

---

## 🔧 توسعه

### ساختار پروژه

```
Ntk.Tools.AutoType/
├── .github/
│   └── workflows/
│       └── build-and-release.yml    # CI/CD Pipeline
├── Program.cs                        # کد اصلی برنامه
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

# Release
dotnet build -c Release

# Publish (Self-contained)
dotnet publish -c Release -r win-x64 --self-contained true
```

### اجرای تست‌ها

```bash
# در حال حاضر تست واحد وجود ندارد
# برای افزودن تست، از xUnit یا NUnit استفاده کنید
```

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
- **Email**: [your-email@example.com]

---

## 🎯 Roadmap

- [ ] افزودن پشتیبانی از Hotkeys برای توقف/شروع
- [ ] افزودن GUI برای تنظیمات
- [ ] افزودن Logging به فایل
- [ ] افزودن پشتیبانی از چندین متن
- [ ] افزودن پشتیبانی از Scheduled Tasks

---

<div align="center">

**ساخته شده با ❤️ توسط NTK**

[⭐ Star این پروژه را بدهید](https://github.com/yourusername/Ntk.Tools.AutoType)

</div>
