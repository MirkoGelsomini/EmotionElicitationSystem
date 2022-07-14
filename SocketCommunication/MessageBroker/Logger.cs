using System;
using System.IO;
using System.Diagnostics;

namespace LogManager
{
    class Logger
    {

        bool _enabled;
        string LogPath = System.Reflection.Assembly.GetEntryAssembly().Location.Replace("MessageBroker.dll", "") + @"logs\";
        string ProjectName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;

        public Logger(bool enabled = true)
        {
            _enabled = enabled;

            try
            {
                Directory.CreateDirectory(LogPath);
                LogPath = LogPath + "log.txt";
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot create Log folder: " + LogPath + " (" + DateTime.Now.ToString("h:mm:ss") + ")");
            }
        }

        enum Colors { Success = ConsoleColor.Green, Alert = ConsoleColor.Magenta, Warn = ConsoleColor.DarkYellow, Error = ConsoleColor.DarkRed, Generic = ConsoleColor.White, Info = ConsoleColor.Cyan };
        public void Log(string message, string type)
        {
            if (_enabled)
            {
                ConsoleColor color;
                if (type == null)
                {
                    color = ConsoleColor.White;
                }
                else
                {
                    color = (ConsoleColor)Enum.Parse(typeof(Colors), type);
                }

                Console.ResetColor();
                Console.ForegroundColor = color;
                Console.WriteLine(message + " (" + DateTime.Now.ToString("H:mm:ss") + ")");
                Console.ResetColor();

                if (type == "Error")
                {
                    using (StreamWriter w = File.AppendText(LogPath))
                    {
                        w.WriteLine(DateTime.Now.ToString("dd/MM/yyyy H:mm:ss") + "\t-\t" + message);
                    }
                }
            }
        }

        public void Log(string message)
        {
            Log(message, null);
        }

        public void Clear()
        {
            Console.Clear();
        }
    }
}
