
using System.Runtime.InteropServices;
using System.Text;

namespace Ntk.Tools.AutoType
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("user32.dll")]
        static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll")]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        static extern bool EmptyClipboard();

        [DllImport("kernel32.dll")]
        static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll")]
        static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        static extern bool GlobalUnlock(IntPtr hMem);

        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint CF_UNICODETEXT = 13;
        private const uint GMEM_MOVEABLE = 0x0002;
        private const uint GMEM_ZEROINIT = 0x0040;

        private static string text = string.Empty;
        private static int sleepMinutes;
        private static int maxExecutions;
        private static int currentExecution = 0;

        static void Main(string[] args)
        {
            // Set console encoding to UTF-8 for proper Persian character display
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            
            // Get parameters from command line if provided, otherwise ask user
            if (args.Length >= 3)
            {
                // Check if first argument is a filename
                text = LoadTextFromInput(args[0]);
                
                if (!int.TryParse(args[1], out sleepMinutes) || sleepMinutes <= 0)
                {
                    Console.Error.WriteLine("Error: Time interval must be a positive number.");
                    Environment.Exit(1);
                }

                if (!int.TryParse(args[2], out maxExecutions) || maxExecutions < 0)
                {
                    Console.Error.WriteLine("Error: Number of executions must be a non-negative number (0 for unlimited).");
                    Environment.Exit(1);
                }
            }
            else
            {
                // Get input line by line
                Console.WriteLine("=== Ntk.Tools.AutoType - Auto Type Program ===");
                Console.WriteLine();
                
                // Get text
                Console.Write("Enter text to type (or filename like my.txt): ");
                string? inputText = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(inputText))
                {
                    Console.Error.WriteLine("Error: Text cannot be empty.");
                    Environment.Exit(1);
                }
                
                // Check if input is a filename
                text = LoadTextFromInput(inputText);
                if (string.IsNullOrWhiteSpace(text))
                {
                    Console.Error.WriteLine("Error: Text cannot be empty.");
                    Environment.Exit(1);
                }
                
                // Get time interval
                Console.Write("Enter time interval in minutes: ");
                string? intervalInput = Console.ReadLine();
                if (!int.TryParse(intervalInput, out sleepMinutes) || sleepMinutes <= 0)
                {
                    Console.Error.WriteLine("Error: Time interval must be a positive number.");
                    Environment.Exit(1);
                }
                
                // Get max executions
                Console.Write("Enter number of executions (0 for unlimited): ");
                string? executionsInput = Console.ReadLine();
                if (!int.TryParse(executionsInput, out maxExecutions) || maxExecutions < 0)
                {
                    Console.Error.WriteLine("Error: Number of executions must be a non-negative number (0 for unlimited).");
                    Environment.Exit(1);
                }
                
                Console.WriteLine();
            }

            Console.WriteLine("Auto Type program started...");
            Console.WriteLine($"Text: {text}");
            Console.WriteLine($"Time interval: {sleepMinutes} minutes");
            Console.WriteLine($"Max executions: {(maxExecutions == 0 ? "Unlimited" : maxExecutions.ToString())}");
            Console.WriteLine("Press Ctrl+C to stop the program");
            Console.WriteLine("\nStarting in 5 seconds...");

            try
            {
                Thread.Sleep(5000); // 5 seconds delay for preparation
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine("Program stopped.");
                return;
            }

            int sleepMillis = sleepMinutes * 60 * 1000; // Convert minutes to milliseconds

            while (true)
            {
                try
                {
                    if (maxExecutions > 0 && currentExecution >= maxExecutions)
                    {
                        Console.WriteLine($"\nMaximum executions reached ({maxExecutions} times).");
                        break;
                    }

                    currentExecution++;
                    Console.WriteLine($"\n[{currentExecution}] Typing... ({DateTime.Now:HH:mm:ss})");
                    
                    TypeText(text);

                    // Press Enter
                    PressKey(0x0D); // VK_RETURN
                    ReleaseKey(0x0D);

                    string maxExecText = maxExecutions == 0 ? "Unlimited" : maxExecutions.ToString();
                    Console.WriteLine($"Typing completed. ({currentExecution}/{maxExecText}) Waiting {sleepMinutes} minutes...");
                    
                    Thread.Sleep(sleepMillis);
                }
                catch (ThreadInterruptedException)
                {
                    Console.WriteLine("\nProgram stopped.");
                    break;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Error typing: {e.Message}");
                }
            }
        }

        private static void TypeText(string text)
        {
            // Use Clipboard to type text (supports Persian and other Unicode characters)
            try
            {
                // Try using Windows Forms Clipboard first (more reliable for Unicode)
                try
                {
                    Clipboard.SetText(text, TextDataFormat.UnicodeText);
                }
                catch
                {
                    // Fallback to native API if Windows Forms fails
                    SetClipboardText(text);
                }
                
                Thread.Sleep(100); // Delay to ensure Clipboard is set

                // Use Ctrl+V to paste
                PressKey(0x11); // VK_CONTROL
                Thread.Sleep(20);
                PressKey(0x56); // VK_V
                Thread.Sleep(20);
                ReleaseKey(0x56);
                Thread.Sleep(20);
                ReleaseKey(0x11);

                Thread.Sleep(300); // Delay to ensure paste is complete
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error typing with Clipboard: {e.Message}");
            }
        }

        private static void SetClipboardText(string text)
        {
            // Try multiple times to ensure clipboard is available
            int attempts = 0;
            const int maxAttempts = 10;
            
            while (attempts < maxAttempts)
            {
                if (OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        EmptyClipboard();

                        // Calculate required size: (text length + 1) * 2 bytes for Unicode
                        int size = (text.Length + 1) * 2;
                        IntPtr hGlobal = GlobalAlloc(GMEM_MOVEABLE | GMEM_ZEROINIT, (UIntPtr)size);
                        
                        if (hGlobal != IntPtr.Zero)
                        {
                            IntPtr pGlobal = GlobalLock(hGlobal);
                            if (pGlobal != IntPtr.Zero)
                            {
                                try
                                {
                                    // Copy Unicode string to memory
                                    byte[] bytes = Encoding.Unicode.GetBytes(text + "\0");
                                    Marshal.Copy(bytes, 0, pGlobal, bytes.Length);
                                    
                                    GlobalUnlock(hGlobal);
                                    
                                    // Set clipboard data
                                    IntPtr result = SetClipboardData(CF_UNICODETEXT, hGlobal);
                                    if (result != IntPtr.Zero)
                                    {
                                        // Success - don't free hGlobal, clipboard owns it now
                                        return;
                                    }
                                    else
                                    {
                                        // Failed to set clipboard data, free memory
                                        GlobalLock(hGlobal);
                                        GlobalUnlock(hGlobal);
                                        // Note: We can't free hGlobal here if SetClipboardData succeeded
                                    }
                                }
                                catch
                                {
                                    GlobalUnlock(hGlobal);
                                }
                            }
                        }
                    }
                    finally
                    {
                        CloseClipboard();
                    }
                }
                
                attempts++;
                if (attempts < maxAttempts)
                {
                    Thread.Sleep(50);
                }
            }
            
            throw new Exception("Failed to set clipboard text after multiple attempts");
        }

        private static string LoadTextFromInput(string input)
        {
            // Check if input is a filename (ends with .txt or contains path)
            if (input.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || 
                input.Contains("\\") || 
                input.Contains("/") ||
                File.Exists(input))
            {
                try
                {
                    // Try to read from file
                    if (File.Exists(input))
                    {
                        string fileContent = File.ReadAllText(input, Encoding.UTF8);
                        Console.WriteLine($"Loaded text from file: {input}");
                        Console.WriteLine($"File content length: {fileContent.Length} characters");
                        return fileContent.TrimEnd('\r', '\n'); // Remove trailing newlines
                    }
                    else
                    {
                        // Try to find file in current directory
                        string currentDir = Directory.GetCurrentDirectory();
                        string filePath = Path.Combine(currentDir, input);
                        if (File.Exists(filePath))
                        {
                            string fileContent = File.ReadAllText(filePath, Encoding.UTF8);
                            Console.WriteLine($"Loaded text from file: {filePath}");
                            Console.WriteLine($"File content length: {fileContent.Length} characters");
                            return fileContent.TrimEnd('\r', '\n');
                        }
                        else
                        {
                            Console.Error.WriteLine($"Warning: File '{input}' not found. Using as text.");
                            return input;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error reading file '{input}': {ex.Message}");
                    Console.Error.WriteLine("Using input as text instead.");
                    return input;
                }
            }
            
            // Not a filename, use as text
            return input;
        }

        private static void PressKey(byte keyCode)
        {
            keybd_event(keyCode, 0, 0, UIntPtr.Zero);
        }

        private static void ReleaseKey(byte keyCode)
        {
            keybd_event(keyCode, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }
}

