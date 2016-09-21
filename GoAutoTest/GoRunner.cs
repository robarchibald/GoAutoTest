using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GoAutoTest
{
  internal static class GoRunner
  {
    public static ProcessOutput RunGoTool(string workingDirectory, string arguments)
    {
      var root = Environment.GetEnvironmentVariable("GOROOT");
      if (string.IsNullOrWhiteSpace(root))
        root = @"C:\Go";
      const int timeout = 11000; // timeout the entire process if it runs > 11 seconds
      using (var process = new Process
                           {
                             StartInfo =
                             {
                               FileName = Path.Combine(root, @"bin\go.exe"),
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

          return new ProcessOutput
                 {
                   StandardError = error.ToArray(),
                   StandardOutput = stdOutList.ToArray()
                 };
        }
      }
    }
  }
}