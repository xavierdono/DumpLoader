using System;

namespace DumpLoader
{
    public class LogConsole
    {
        static public void LogInformation(string texte, string message)
        {
            Console.ResetColor();
            Console.WriteLine();
            Console.Write($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {texte}");
            
            if (!string.IsNullOrEmpty(message))
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write(message);
            }
        }

        static public void LogError(Exception ex)
        {
            Console.ResetColor();
            Console.WriteLine();
            Console.Write($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Error: ");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(ex.Message);
        }
    }
}