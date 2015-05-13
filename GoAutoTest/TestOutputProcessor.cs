using System;
using System.Collections.Generic;
using System.Linq;

namespace GoAutoTest
{
  public class TestOutputProcessor
  {
    public List<ConsoleOutput> ConsoleOutputItems { get; set; }
    public bool HasError { get; set; }
    public string CoverageLine { get; set; }
    public decimal CoveragePercent { get; set; }
    public bool BuildFailed { get; set; }

    public TestOutputProcessor(ProcessOutput output)
    {
      ConsoleOutputItems = new List<ConsoleOutput>();
      foreach (var line in output.StandardOutput)
      {
        ConsoleOutputItems.Add(new ConsoleOutput
                               {
                                 Type = line.Contains("FAIL") ? OutputType.TestFail :
                                         line.Contains("PASS") ? OutputType.TestPass :
                                         line.Contains("SKIP") ? OutputType.TestSkip :
                                         line.Contains("coverage:") ? OutputType.TestCoverageTotal :
                                         line.Contains("===") ? OutputType.Header :
                                         OutputType.Other,
                                 Message = line
                               });
      }
      HasError = ConsoleOutputItems.Any(i => i.Type == OutputType.TestFail);
      BuildFailed = ConsoleOutputItems.Any(i => i.Message.Contains("build failed"));
      var coverage = ConsoleOutputItems.SingleOrDefault(i => i.Type == OutputType.TestCoverageTotal);
      CoverageLine = coverage != null ? coverage.Message: "";
      CoveragePercent = coverage != null ? Convert.ToDecimal(CoverageLine.SubstringBetween("coverage: ", "%")) : -1;
    }
  }
}
