using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TTSAutomate
{
    public static class Logger
    {
        public static void Log(String message)
        {
            using (StreamWriter sw = new StreamWriter(File.OpenWrite("log.txt")))
            {
                sw.WriteLine(message);
            }
        }
    }
}
