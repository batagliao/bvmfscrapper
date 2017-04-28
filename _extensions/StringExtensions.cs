using System;
using System.Collections.Generic;
using System.Text;

namespace bvmfscrapper
{
    public static class StringExtensions
    {
        static char[] NUMS = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public static int IndexOfNum(this string s, int startindex = 0)
        {
            return s.IndexOfAny(NUMS, startindex);
        }

        public static int LastIndexOfNum(this string s, int startindex = 0)
        {
            if(startindex == 0)
            {
                startindex = s.Length - 1;
            }
            return s.LastIndexOfAny(NUMS, startindex);
        }
    }
}
