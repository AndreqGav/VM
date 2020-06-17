using System;
using System.IO;
using System.Linq;

namespace WelcomeToVichMat.Extensions
{
    public static class StreamReaderExtensions
    {
        public static string[] ReadLineAndSplit(this StreamReader reader)
        {
            var str = (reader.ReadLine()?.Split("              ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) ??
                      new string[] { })
                          .Where(itm => !string.IsNullOrWhiteSpace(itm))
                          .ToList();
            str = str.Select(itm => itm.Replace(".", ",")).ToList();

            return str.ToArray();
        }
    }
}
