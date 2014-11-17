using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GibbsLDA.NET
{
    public class Conversion
    {
        public static string ZeroPad(int number, int width)
        {
            var result = new StringBuilder();
            for (int i = 0; i < number.ToString().Length; i++)
            {
                result.Append("0");
            }
            result.Append(number.ToString());
            return result.ToString();
        }
    }
}