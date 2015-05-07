using System;
using System.IO;

namespace GoAutoTest
{
  class Program
  {
    private static DateTime lastFileChangeTime;

    static void Main(string[] args)
    {
      string path = args.Length > 0 ? args[0] : null;
      while (!Directory.Exists(path) && !File.Exists(path))
      {
        Console.WriteLine();
        Console.Write("Enter directory name to watch: ");
        path = Console.ReadLine();
      }

      lastFileChangeTime = DateTime.Now;
      var watcher = new DirectoryMonitor(path, "*.go");
      watcher.Change += OnWatcherChange;

      while (true)
      {
        System.Threading.Thread.Sleep(100);
      }

    }

    static void OnWatcherChange(string path)
    {
      RunTests(Path.GetDirectoryName(path));
    }

    private static void RunTests(string path)
    {
      var process = new System.Diagnostics.Process
      {
        StartInfo =
        {
          FileName = @"C:\Go\bin\go.exe",
          Arguments = "test -v -short -coverprofile cover.out",
          CreateNoWindow = true,
          WorkingDirectory = path,
          UseShellExecute = false,
          RedirectStandardOutput = true,
          RedirectStandardError = true
        }
      };
      process.Start();
      process.WaitForExit();
      Console.Clear();
      Console.ForegroundColor = ConsoleColor.White;
      var hasError = false;
      string line = null;
      while (!process.StandardOutput.EndOfStream)
      {
        line = process.StandardOutput.ReadLine();
        if (line.Contains("FAIL"))
        {
          Console.ForegroundColor = ConsoleColor.Red;
          hasError = true;
        }
        else if (line.Contains("PASS"))
        {
          Console.ForegroundColor = ConsoleColor.DarkGreen;
        }
        else if (line.Contains("SKIP"))
        {
          Console.ForegroundColor = ConsoleColor.DarkYellow;
        }
        else
        {
          Console.ForegroundColor = ConsoleColor.White;
        }

        if (line.Contains("---"))
          Console.WriteLine(line);

        if (line.Contains("coverage:"))
        {
          var percent = Convert.ToDecimal(SubstringBetween(line, "coverage: ", "%"));
          if (percent < 70)
          {
            Console.ForegroundColor = ConsoleColor.Red;
            hasError = true;
          }
          else if (percent < 85)
            Console.ForegroundColor = ConsoleColor.DarkYellow;
          else
            Console.ForegroundColor = ConsoleColor.DarkGreen;

          Console.WriteLine();
          Console.WriteLine(line);
        }
      }
      Console.WriteLine(line);
      Console.ForegroundColor = ConsoleColor.Red;
      var errors = process.StandardError.ReadToEnd();
      Console.WriteLine(errors);

      Console.WriteLine(process.StandardOutput.ReadToEnd());

      if (hasError || !string.IsNullOrWhiteSpace(errors))
      {
        Console.Beep(660, 200);
        Console.Beep(440, 200);
      }
    }

    private static string SubstringBetween(string source, string start, string end)
    {
      var startIndex = source.IndexOf(start, StringComparison.CurrentCulture);
      if (startIndex != -1)
      {
        var endIndex = source.IndexOf(end, startIndex, StringComparison.CurrentCultureIgnoreCase);
        if (endIndex != -1)
        {
          return source.Substring(startIndex + start.Length, endIndex - startIndex - start.Length);
        }

        return source.Substring(startIndex + start.Length);
      }

      return source;
    }
  }
}
