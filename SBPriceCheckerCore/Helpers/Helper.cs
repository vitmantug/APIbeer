using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBPriceCheckerCore.Helpers
{
    public class Helper
    {
        private static string DECIMAL_SEPARATOR = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        private static string GROUP_SEPARATOR = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator;

        public double ConvertPTNumberStrToDouble(string strValue)
        {
            if (strValue.Contains(GROUP_SEPARATOR))
            {
                string valueRep = strValue.Replace(GROUP_SEPARATOR, DECIMAL_SEPARATOR);
                return Convert.ToDouble(valueRep);
            }
            else
            {
                if (DECIMAL_SEPARATOR.Equals(","))
                    strValue = strValue.Replace(".", ",");

                return Convert.ToDouble(strValue);
            }
        }
    }
}
