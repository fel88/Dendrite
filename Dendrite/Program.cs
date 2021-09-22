using Dagre;
using System;
using System.Windows.Forms;

namespace Dendrite
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm = new Mdi();
                 
            //DagreTester.Test11();

            Application.Run(MainForm);
        }       

        public static Mdi MainForm;
    }
}
