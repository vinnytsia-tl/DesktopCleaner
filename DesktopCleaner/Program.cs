using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DesktopCleaner
{
    internal class Program
    {
        private static string LogDirectory = Path.GetTempPath() + Assembly.GetExecutingAssembly().GetName().Name + @"\";
        private static string LogPath = LogDirectory + "log.log";

        private static StreamWriter logger;
        private static HashSet<string> proccessed_dir;

        [STAThread]
        static void Main()
        {
            proccessed_dir = new HashSet<string>();

            try
            {
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);
                logger = new StreamWriter(LogPath, true);
                logger.WriteLine(LogFormat("info", "{0} started. Version: {1}",
                    Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Version));

                // Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                CleanDirectory(new DirectoryInfo("C:\\Users\\Administrator\\Desktop\\test"));
            }
            catch (Exception ex)
            {
                logger.WriteLine(LogFormat("error", ex.ToString()));
            }
            finally
            {
                logger?.Close();
            }
        }

        private static string LogFormat(string type, string format, params object[] args)
        {
            string date = DateTime.Now.ToString();
            string message = string.Format(format, args);

            return string.Format("[{0}][{1}]{2}", date, type.ToUpper(), message);
        }

        private static void CleanDirectory(DirectoryInfo directory)
        {
            logger.WriteLine(LogFormat("info", "Clean directory {0}", directory.FullName));

            if (directory.Exists)
            {
                foreach (FileInfo file in directory.GetFiles())
                    DeleteFile(file.FullName);

                foreach (DirectoryInfo dir in directory.GetDirectories())
                    DeleteDirectory(new DirectoryInfo($"{dir.FullName}{Path.DirectorySeparatorChar}"));
            }
            else
                Directory.CreateDirectory(directory.FullName);
        }

        private static void DeleteDirectory(DirectoryInfo directory)
        {
            if (proccessed_dir.Contains(directory.FullName))
            {
                logger.WriteLine(LogFormat("warn", "Delete directory {0} already was called. Exiting...", directory.FullName));
                return;
            }

            logger.WriteLine(LogFormat("info", "Delete directory {0}", directory.FullName));
            proccessed_dir.Add(directory.FullName);

            try
            {
                if (directory.Exists)
                {
                    CleanDirectory(directory);
                    directory.Delete(true);
                }

                logger.WriteLine(LogFormat("info", "Directory {0} was deleted", directory.FullName));
            }
            catch (Exception ex)
            {
                logger.WriteLine(LogFormat("error", ex.ToString()));
            }
        }

        private static void DeleteFile(string file)
        {
            logger.WriteLine(LogFormat("info", "Delete file {0}", file));

            try
            {
                FileInfo fileInfo = new FileInfo(file);

                if (fileInfo.Exists)
                {
                    ResetFileAttributes(fileInfo.FullName);
                    fileInfo.Delete();
                }

                logger.WriteLine(LogFormat("info", "File {0} was deleted", file));
            }
            catch (Exception ex)
            {
                logger.WriteLine(LogFormat("error", ex.ToString()));
            }
        }

        private static void ResetFileAttributes(string file)
        {
            try
            {
                if (File.Exists(file))
                    File.SetAttributes(file, FileAttributes.Normal);
            }
            catch (Exception ex)
            {
                logger.WriteLine(LogFormat("error", ex.ToString()));
            }
        }
    }
}
