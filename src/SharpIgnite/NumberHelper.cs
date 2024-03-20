using System;

namespace SharpIgnite
{
    public class NumberHelper
    {
        public static int Random()
        {
            return (new Random(unchecked((int)DateTime.Now.Ticks))).Next();
        }
    }
}
