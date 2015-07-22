using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoAutoTest
{
  public class ConsoleOutput: IOutputType
  {
    public string Message { get; set; }
    public OutputType Type { get; set; }

    public override string ToString()
    {
      return Message;
    }
  }

  public enum OutputType
  {
    Error,
    TestFail, 
    TestPass,
    TestSkip,
    TestCoverageTotal,
    Header,
    Summary,
    Other
  }
}
