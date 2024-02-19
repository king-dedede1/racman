using System;
using System.Windows.Forms;
using System.Reflection;

using AutoUpdaterDotNET;

namespace racman
{
    static class Program
    {


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        
        [STAThread]
        static void Main(string[] args)
        {
#if !DEBUG
            AutoUpdater.Start("https://MichaelRelaxen.github.io/racman/update.xml");
            AutoUpdater.RunUpdateAsAdmin = false;
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Start();
        }

        public static Racman racman;
        public static AttachGameForm AttachGameForm;
        public static void Start()
        {
            racman = new Racman();
            racman.ShowConnectDialog();
            Application.Run(racman.MainForm);
        }
    }
}
