using System;
using System.Runtime.InteropServices;

namespace ConsoleApp
{
    class Program
    {
        [DllImport("libminiprotoc.dll")]
        public static extern bool generate();
        static void Main(string[] args)
        {
            generate();
            Console.WriteLine("Hello World!");
        }
    }
}
