using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoAutoTest
{
  public class ConsoleOutput
  {
    public string Message { get; set; }
    public OutputType Type { get; set; }
  }

  public enum OutputType
  {
    TestFail, 
    TestPass,
    TestSkip,
    TestCoverageTotal,
    Header,
    Other
  }
}
