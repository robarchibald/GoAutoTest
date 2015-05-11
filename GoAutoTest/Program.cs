using System;
using System.IO;

namespace GoAutoTest
{
  class Program
  {
    static void Main(string[] args)
    {
      string path = args.Length > 0 ? args[0] : null;
      while (!Directory.Exists(path) && !File.Exists(path))
      {
        Console.WriteLine();
        Console.Write("Enter directory name to watch: ");
        path = Console.ReadLine();
      }

      var watcher = new DirectoryMonitor(path, "*.go");
      watcher.Change += OnWatcherChange;

      while (true)
      {
        System.Threading.Thread.Sleep(100);
      }

    }

    static void OnWatcherChange(string path)
    {
      RunTests(path);
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
          WorkingDirectory = Path.GetDirectoryName(path),
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
      string coverageLine = null;
      decimal coveragePercent = 0;
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
          coveragePercent = Convert.ToDecimal(SubstringBetween(line, "coverage: ", "%"));
          coverageLine = line;
        }
      }
      Console.WriteLine(line);
      Console.ForegroundColor = ConsoleColor.Red;
      var errors = process.StandardError.ReadToEnd();
      Console.WriteLine(errors);

      Console.WriteLine(process.StandardOutput.ReadToEnd());

      process.StartInfo.Arguments = "tool cover -func=cover.out";
      process.Start();
      process.WaitForExit();
      string shortFile = Path.GetFileNameWithoutExtension(path).Replace("_test", "");
      while (!process.StandardOutput.EndOfStream)
      {
        line = process.StandardOutput.ReadLine();
        if (line.Contains(shortFile))
        {
          var info = line.Substring(line.IndexOf("\t", StringComparison.CurrentCulture)).Trim();
          var functionPercentage = Convert.ToDecimal(SubstringBetween(info, "\t", "%"));
          PrintCoverage(functionPercentage, info);
        }
      }

      var coverageError = PrintCoverage(coveragePercent, coverageLine);
      if (hasError || coverageError || !string.IsNullOrWhiteSpace(errors))
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

    private static bool PrintCoverage(decimal percentage, string message)
    {
      var hasError = false;
      if (percentage < 70)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        hasError = true;
      }
      else if (percentage < 85)
      {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
      }
      else
      {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
      }
      
      Console.WriteLine(message);
      return hasError;
    }
  }
}
