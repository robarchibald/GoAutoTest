using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoAutoTest
{
  public static class CodeCoverage
  {
    public static bool Print(string line)
    {
      var items = line.Split("\t".ToCharArray());
      var file = Path.GetFileName(line.Substring(0, items[0].IndexOf(":", StringComparison.CurrentCulture)));
      //var lineNumber = Convert.ToInt32(items[0].Substring(file.Length + 1));
      var method = string.Join("", items.Skip(1).Take(items.Length-2));
      decimal percentage;
      decimal.TryParse(items.Last().Replace("%", ""), out percentage);
      return PrintCoverage(percentage, file.PadRight(35) + method.PadRight(30) + percentage + "%");
    }

    private static bool PrintCoverage(decimal percentage, string message)
    {
      var hasError = false;
      if (percentage < 75)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        hasError = true;
      }
      else if (percentage < 100)
      {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(message);
      }

      return hasError;
    }
  }
}
