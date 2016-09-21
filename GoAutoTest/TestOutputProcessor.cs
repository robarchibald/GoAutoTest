using System;
using System.Collections.Generic;
using System.Linq;

namespace GoAutoTest
{
  public class TestOutputProcessor
  {
    public List<ConsoleOutput> ConsoleOutputItems { get; set; }
    public List<FileSummary> Summary { get; set; }
    public bool HasError { get; set; }
    public bool BuildFailed { get; set; }

    public TestOutputProcessor(ProcessOutput output)
    {
      ConsoleOutputItems = new List<ConsoleOutput>();
      Summary = new List<FileSummary>();

      var passed = 0;
      var failed = 0;
      var skipped = 0;
      var summaryConsoleItems = new List<ConsoleOutput>();
      foreach (var line in output.StandardOutput)
      {
        var type = line.Contains("FAIL:") ? OutputType.TestFail
                 : line.Contains("PASS:") ? OutputType.TestPass
                 : line.Contains("SKIP:") ? OutputType.TestSkip
                 : line.Contains("===") ? OutputType.Header
                 : line.Contains("panic:") ? OutputType.Error
                 : line == "PASS" ? OutputType.Header
                 : line.Contains("ok  \t") ? OutputType.Summary
                 : line.Contains("?   \t") ? OutputType.Summary
                 : line.Contains("FAIL\t") ? OutputType.Summary
                 : OutputType.Other;
        decimal runSeconds;
        decimal.TryParse(line.SubstringBetween("(", "s)"), out runSeconds);
        var consoleOutput = new ConsoleOutput {Message = line, Type = type, RunSeconds = runSeconds};
        ConsoleOutputItems.Add(consoleOutput);

        summaryConsoleItems.Add(consoleOutput);
        switch (type)
        {
          case OutputType.TestPass:
            passed++;
            break;
          case OutputType.TestFail:
            failed++;
            break;
          case OutputType.TestSkip:
            skipped++;
            break;
          case OutputType.Summary:
            Summary.Add(new FileSummary(line, passed, failed, skipped, summaryConsoleItems));
            passed = skipped = failed = 0;
            summaryConsoleItems = new List<ConsoleOutput>();
            break;
        }
      }
      HasError = ConsoleOutputItems.Any(i => i.Type == OutputType.TestFail);
      BuildFailed = ConsoleOutputItems.Any(i => i.Message.Contains("build failed"));
    }
  }

  public interface IOutputType
  {
    OutputType Type { get; }
  }

  public class FileSummary: IOutputType
  {
    public string Status { get; private set; }
    public string Filename { get; private set; }
    public string Result { get; private set; }
    public int TestsRun { get; private set; }
    public int TestsPassed { get; private set; }
    public int TestsSkipped { get; private set; }
    public int TestsFailed { get; private set; }
    public bool Success { get { return TestsFailed == 0; } }
    public OutputType Type { get; private set; }
    public List<ConsoleOutput> Lines { get; private set; }

    public FileSummary(string message, int passed, int failed, int skipped, List<ConsoleOutput> lines)
    {
      Status = message.Substring(0, 4).Trim();
      Status = Status == "ok" ? "PASS" : Status == "?" ? "SKIP" : "FAIL";
      Filename = string.Join("", message.Substring(5).TakeWhile(i => !char.IsWhiteSpace(i)));
      Result = message.Substring(5 + Filename.Length + 1);
      TestsPassed = passed;
      TestsFailed = failed;
      TestsSkipped = skipped;
      TestsRun = passed + skipped + failed;
      Type = Status == "FAIL" ? OutputType.TestFail : Status == "PASS" ? OutputType.TestPass : OutputType.TestSkip;
      Lines = lines;
    }

    public override string ToString()
    {
      return string.Format("---{0}    {1} {2} Passed, {3} Failed, {4} Skipped   {5}", Status, Filename.PadRight(40), TestsPassed.ToString().PadLeft(3), TestsFailed.ToString().PadLeft(3), TestsSkipped.ToString().PadLeft(3), Result);
    }
  }
}
