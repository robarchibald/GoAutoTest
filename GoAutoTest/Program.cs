using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GoAutoTest
{
  class Program
  {
    private const string runTestsArgs = "test -v -short -timeout 5s"; // panic if test runs longer than 5 seconds
    private const string runCoverageArgs = "test -short -coverprofile cover.out -timeout 5s";
    private const string coverageArgs = "tool cover -func=cover.out";

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

    static void OnWatcherChange(string folder)
    {
      RunTests(folder);
    }

    private static async void RunTests(string folder)
    {
      // asynchronously run test and coverage tasks
      var tests = Task.Run(() => GoRunner.RunGoTool(folder, runTestsArgs));
      var coverage = Task.Run(() => Coverage(folder));

      if (!ProcessTests(await tests) && !ProcessCodeCoverage(await coverage))
        BeepSuccess();
    }

    private static ProcessOutput Coverage(string folder)
    {
      var profile = GoRunner.RunGoTool(folder, runCoverageArgs); // create cover profile
      if (profile.StandardError.Any())
        return profile;

      return GoRunner.RunGoTool(folder, coverageArgs);
    }

    private static bool ProcessCodeCoverage(ProcessOutput output)
    {
      if (output.StandardError.Any())
      {
        ShowBuildError(output);
        return true;
      }

      var coverageError = false;
      foreach (var line in output.StandardOutput)
      {
        var hasError = CodeCoverage.Print(line);
        coverageError = line.Contains("(statements)") ? hasError : coverageError;
      }
      return coverageError || output.StandardError.Any();
    }

    private static bool ProcessTests(ProcessOutput output)
    {
      Console.Clear();
      Console.ForegroundColor = ConsoleColor.White;
      if (output.StandardError.Any())
      {
        ShowBuildError(output);
        return true;
      }

      var processor = new TestOutputProcessor(output);
      foreach (var summary in processor.Summary)
      {
        if (summary.Status == "FAIL")
        {
          foreach (var item in summary.Lines)
            WriteLine(item);
        }
        else
        {
          foreach (var item in summary.Lines.Where(i => i.Type == OutputType.Other || i.RunSeconds > 0.1M))
            WriteLine(item);
        }
        WriteLine(summary);
      }

      Console.WriteLine("");
      return processor.HasError;
    }

    private static void WriteLine(IOutputType item)
    {
      Console.ForegroundColor = item.Type == OutputType.TestFail || item.Type == OutputType.Error ? ConsoleColor.Red
                              : item.Type == OutputType.TestPass ? ConsoleColor.DarkGreen
                              : item.Type == OutputType.TestSkip ? ConsoleColor.DarkYellow
                              : ConsoleColor.White;

      if (item.Type == OutputType.TestFail || item.Type == OutputType.Error || item.Type == OutputType.TestPass || item.Type == OutputType.TestSkip ||
          item.Type == OutputType.Other)
        Console.WriteLine(item.ToString());
    }

    private static void ShowBuildError(ProcessOutput output)
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine(string.Join(Environment.NewLine, output.StandardError));

      BeepError();
    }

    private static void BeepError()
    {
      Console.Beep(660, 200);
      Console.Beep(440, 200);
    }

    private static void BeepSuccess()
    {
      Console.Beep(660, 50);
      Console.Beep(1320, 50);
    }
  }
}
