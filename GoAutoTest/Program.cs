using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GoAutoTest
{
  class Program
  {
    private const string runTestsArgs = "test ./... -v -short -timeout 5s";
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

    static void OnWatcherChange(string path)
    {
      RunTests(path);
    }

    private static void RunTests(string path)
    {
      var workingDirectory = Path.GetDirectoryName(path);
      var srcDirectory = workingDirectory.SubstringTerminatedAt("src\\");
      var folder = workingDirectory.SubstringBetween(srcDirectory, "\\");
      var output = RunGoTool(Path.Combine(srcDirectory, folder), runTestsArgs);
      
      Console.Clear();
      Console.ForegroundColor = ConsoleColor.White;
      var processor = new TestOutputProcessor(output);
      if (processor.BuildFailed)
      {
        ShowBuildError(output);
        return;
      }

      foreach (var item in processor.ConsoleOutputItems)
      {
        Console.ForegroundColor = 
          item.Type == OutputType.TestFail ? ConsoleColor.Red : 
          item.Type == OutputType.TestPass ? ConsoleColor.DarkGreen : 
          item.Type == OutputType.TestSkip ? ConsoleColor.DarkYellow : 
          ConsoleColor.White;

        if (item.Type == OutputType.TestFail || item.Type == OutputType.TestPass || item.Type == OutputType.TestSkip || item.Type == OutputType.Other)
          Console.WriteLine(item.Message);
      }

      if (!string.IsNullOrWhiteSpace(output.StandardError))
      {
        if (output.StandardError.Contains("goroutine"))
        {
          Console.WriteLine(output.StandardError.Substring(0, output.StandardError.IndexOf("goroutine", StringComparison.CurrentCultureIgnoreCase)));
          foreach (var line in output.StandardError.Split(Environment.NewLine.ToCharArray()))
          {
            if (line.Contains("endfirst.com"))
              Console.WriteLine(line.SubstringAfter("endfirst.com/"));
          }
        }
        else
        {
          Console.WriteLine(output.StandardError);
        }
      }

      Console.WriteLine("");
      RunGoTool(workingDirectory, runCoverageArgs);
      output = RunGoTool(workingDirectory, coverageArgs);
      var coverageError = false;
      foreach (var line in output.StandardOutput)
      {
        var hasError = CodeCoverage.Print(line);
        coverageError = line.Contains("(statements)") ? hasError: coverageError;
      }

      if (processor.HasError || coverageError || output.StandardError.Any())
      {
        BeepError();
      }
      else
      {
        BeepSuccess();
      }
    }

    private static void ShowBuildError(ProcessOutput output)
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine(output.StandardError);

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

    private static ProcessOutput RunGoTool(string workingDirectory, string arguments)
    {
      const int timeout = 5000;
      using (var process = new Process
                           {
                             StartInfo =
                             {
                               FileName = @"C:\Go\bin\go.exe",
                               Arguments = arguments,
                               CreateNoWindow = true,
                               WorkingDirectory = workingDirectory,
                               UseShellExecute = false,
                               RedirectStandardOutput = true,
                               RedirectStandardError = true
                             }
                           })
      {
        var output = new List<string>();
        var error = new List<string>();

        using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
        using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
        {
          process.OutputDataReceived += (sender, e) =>
                                        {
                                          if (e.Data == null)
                                          {
                                            outputWaitHandle.Set();
                                          }
                                          else
                                          {
                                            output.Add(e.Data);
                                          }
                                        };
          process.ErrorDataReceived += (sender, e) =>
                                       {
                                         if (e.Data == null)
                                         {
                                           errorWaitHandle.Set();
                                         }
                                         else
                                         {
                                           error.Add(e.Data);
                                         }
                                       };

          process.Start();

          process.BeginOutputReadLine();
          process.BeginErrorReadLine();

          var killed = false;
          if (!process.WaitForExit(timeout) || !outputWaitHandle.WaitOne(timeout) || !errorWaitHandle.WaitOne(timeout))
          {
            process.Kill();
            killed = true;
          }
          var stdOutList = new List<string>();
          for (var i = 0; i < output.Count && i < 500; i++)
            stdOutList.Add(output[i]);
          process.CancelErrorRead();
          process.CancelOutputRead();
          if (stdOutList.Count == 500)
            stdOutList.Add("Output Truncated");
          if (killed)
            stdOutList.Add("Process Killed.  ");
          return new ProcessOutput { StandardError = string.Join(Environment.NewLine, error), StandardOutput = stdOutList.ToArray() }; 
        }
      }
    }
  }
}
