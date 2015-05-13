using System;

namespace GoAutoTest
{
  public static class StringHelper
  {
    public static string SubstringBetween(this string source, string start, string end)
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

    public static string SubstringAfter(this string source, string start)
    {
      var startIndex = source.IndexOf(start, StringComparison.CurrentCulture);
      if (startIndex != -1)
      {
        return source.Substring(startIndex + start.Length);
      }

      return source;
    }

  }
}
