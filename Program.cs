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
        private static bool isPaused = false;
        private static ManualResetEventSlim pauseEvent = new ManualResetEventSlim(true);
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static CancellationToken cancellationToken;

        static async Task Main(string[] args)
        {
            cancellationToken = cancellationTokenSource.Token;
            
            // Set console encoding to UTF-8 for proper Persian character display
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            
            // Get parameters from command line if provided, otherwise ask user
            if (args.Length >= 3)
            {
                // Check if first argument is a filename
                text = await LoadTextFromInputAsync(args[0]);
                
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
                string? inputText = await Task.Run(() => Console.ReadLine(), cancellationToken);
                if (string.IsNullOrWhiteSpace(inputText))
                {
                    Console.Error.WriteLine("Error: Text cannot be empty.");
                    Environment.Exit(1);
                }
                
                // Check if input is a filename
                text = await LoadTextFromInputAsync(inputText);
                if (string.IsNullOrWhiteSpace(text))
                {
                    Console.Error.WriteLine("Error: Text cannot be empty.");
                    Environment.Exit(1);
                }
                
                // Get time interval
                Console.Write("Enter time interval in minutes: ");
                string? intervalInput = await Task.Run(() => Console.ReadLine(), cancellationToken);
                if (!int.TryParse(intervalInput, out sleepMinutes) || sleepMinutes <= 0)
                {
                    Console.Error.WriteLine("Error: Time interval must be a positive number.");
                    Environment.Exit(1);
                }
                
                // Get max executions
                Console.Write("Enter number of executions (0 for unlimited): ");
                string? executionsInput = await Task.Run(() => Console.ReadLine(), cancellationToken);
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
            Console.WriteLine("Press P or Space to pause/resume the program");
            Console.WriteLine("\nStarting in 5 seconds...");

            // Setup Ctrl+C handler
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
                pauseEvent.Set(); // Release pause if paused
                Console.WriteLine("\n\nProgram stopping...");
            };

            try
            {
                await Task.Delay(5000, cancellationToken); // 5 seconds delay for preparation
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Program stopped.");
                return;
            }

            // Start keyboard listener task
            var keyboardTask = KeyboardListenerAsync(cancellationToken);

            int sleepMillis = sleepMinutes * 60 * 1000; // Convert minutes to milliseconds

            try
            {
                await MainLoopAsync(sleepMillis, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nProgram stopped.");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error: {e.Message}");
            }
            finally
            {
                cancellationTokenSource.Cancel();
                await keyboardTask;
            }
        }

        private static async Task MainLoopAsync(int sleepMillis, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Wait if paused
                    await Task.Run(() => pauseEvent.Wait(cancellationToken), cancellationToken);
                    
                    if (maxExecutions > 0 && currentExecution >= maxExecutions)
                    {
                        Console.WriteLine($"\nMaximum executions reached ({maxExecutions} times).");
                        break;
                    }

                    currentExecution++;
                    Console.WriteLine($"\n[{currentExecution}] Typing... ({DateTime.Now:HH:mm:ss})");
                    
                    await TypeTextAsync(text, cancellationToken);

                    // Press Enter
                    PressKey(0x0D); // VK_RETURN
                    ReleaseKey(0x0D);

                    string maxExecText = maxExecutions == 0 ? "Unlimited" : maxExecutions.ToString();
                    Console.WriteLine($"Typing completed. ({currentExecution}/{maxExecText}) Waiting {sleepMinutes} minutes...");
                    
                    // Sleep with pause support
                    await SleepWithPauseAsync(sleepMillis, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Error typing: {e.Message}");
                }
            }
        }

        private static async Task TypeTextAsync(string text, CancellationToken cancellationToken)
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
                    await SetClipboardTextAsync(text, cancellationToken);
                }
                
                await Task.Delay(100, cancellationToken); // Delay to ensure Clipboard is set

                // Use Ctrl+V to paste
                PressKey(0x11); // VK_CONTROL
                await Task.Delay(20, cancellationToken);
                PressKey(0x56); // VK_V
                await Task.Delay(20, cancellationToken);
                ReleaseKey(0x56);
                await Task.Delay(20, cancellationToken);
                ReleaseKey(0x11);

                await Task.Delay(300, cancellationToken); // Delay to ensure paste is complete
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error typing with Clipboard: {e.Message}");
            }
        }

        private static async Task SetClipboardTextAsync(string text, CancellationToken cancellationToken)
        {
            // Try multiple times to ensure clipboard is available
            int attempts = 0;
            const int maxAttempts = 10;
            
            while (attempts < maxAttempts && !cancellationToken.IsCancellationRequested)
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
                    await Task.Delay(50, cancellationToken);
                }
            }
            
            throw new Exception("Failed to set clipboard text after multiple attempts");
        }

        private static async Task<string> LoadTextFromInputAsync(string input)
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
                        string fileContent = await File.ReadAllTextAsync(input, Encoding.UTF8);
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
                            string fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
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

        private static async Task KeyboardListenerAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Use Task.Run to check for key availability without blocking
                    var keyCheckTask = Task.Run(() =>
                    {
                        if (Console.KeyAvailable)
                        {
                            return Console.ReadKey(true);
                        }
                        return (ConsoleKeyInfo?)null;
                    }, cancellationToken);
                    
                    var keyInfo = await keyCheckTask;
                    
                    if (keyInfo.HasValue)
                    {
                        // Check for P or Space key to pause/resume
                        if (keyInfo.Value.Key == ConsoleKey.P || keyInfo.Value.Key == ConsoleKey.Spacebar)
                        {
                            await TogglePauseAsync();
                        }
                    }
                    
                    await Task.Delay(100, cancellationToken); // Check every 100ms
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Ignore errors in keyboard listener, but continue checking
                    await Task.Delay(100, cancellationToken);
                }
            }
        }

        private static Task TogglePauseAsync()
        {
            isPaused = !isPaused;
            
            if (isPaused)
            {
                // Pause: reset event (will block MainLoopAsync)
                pauseEvent.Reset();
                Console.WriteLine($"\n[PAUSED] Program paused at {DateTime.Now:HH:mm:ss}. Press P or Space to resume.");
            }
            else
            {
                // Resume: set event (will unblock MainLoopAsync)
                pauseEvent.Set();
                Console.WriteLine($"\n[RESUMED] Program resumed at {DateTime.Now:HH:mm:ss}.");
            }
            
            return Task.CompletedTask;
        }

        private static async Task SleepWithPauseAsync(int milliseconds, CancellationToken cancellationToken)
        {
            int elapsed = 0;
            const int checkInterval = 100; // Check every 100ms
            
            while (elapsed < milliseconds && !cancellationToken.IsCancellationRequested)
            {
                // Check if paused
                if (isPaused)
                {
                    // Wait until resumed
                    await Task.Run(() => pauseEvent.Wait(cancellationToken), cancellationToken);
                }
                else
                {
                    await Task.Delay(checkInterval, cancellationToken);
                    elapsed += checkInterval;
                }
            }
        }
    }
}
