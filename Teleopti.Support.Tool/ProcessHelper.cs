using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Teleopti.Support.Tool
{
    class processHelper
    {
      

        private processHelper() { }

       
       /// <summary>
       /// Starts an application for example notepad
       /// </summary>
       /// <param name="app">For example if you send in C:/someFile.txt it will open the file in notepad if Notepad is the preffered program </param>
        public static void Start(string app)
        {
            Process p = new Process();
            p.StartInfo.FileName = app;
            p.Start();
            p.WaitForExit();
        }
    }
}