using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data;
using System.IO;
using System.Drawing;
using System.Threading;

using Utility;
using SongList;

using NDesk.Options;

namespace KshToVox.window
{
	class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            OptionSet p = new OptionSet() {
                { "p|path=", "The {PATH} of KFC directory.",
                   v => Util.SetKfcPath(v) },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("KshToVox: ");
                Console.WriteLine(e.Message);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

    }
}
