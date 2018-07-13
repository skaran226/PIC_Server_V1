using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Microsoft.Win32;
using System.Diagnostics;

namespace FPS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)//PD - rev 19
        {
            if (args.Length == 1)
            {
                CenCom.iWait = int.Parse(args[0]);//PD - rev 19
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            
            CenCom.StartUp();
            Application.Run();

            //Application.Run(new Form1());
        }
    }
}
