using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraExplorer
{
    public static class Extensions
    {
        public static string EnumerationToParameterName<T>(this T enumeration)
        {
            string name = enumeration.ToString();

            for (int i = 1; i < name.Length; i++)
            {
                char c = name[i];

                if (c >= 'A' && c <= 'Z')
                {
                    name.Remove(i, 1);
                    name.Insert(i++, ((char)(c + 0x30)).ToString());
                    name.Insert(i, " ");
                }
            }

            return name;
        }
    }
}
