using System;

namespace GoAutoTest
{
  public static class StringHelper
  {
    public static string SubstringTerminatedAt(this string source, string termination)
    {
      var startIndex = source.IndexOf(termination, StringComparison.CurrentCulture);
      if (startIndex != -1)
      {
        return source.Substring(0, startIndex+termination.Length);
      }

      return source;
    }

    public static string SubstringBetween(this string source, string start, string end)
    {
      var startIndex = source.IndexOf(start, StringComparison.CurrentCulture);
      if (startIndex != -1)
      {
        var endIndex = source.IndexOf(end, startIndex + start.Length, StringComparison.CurrentCultureIgnoreCase);
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
