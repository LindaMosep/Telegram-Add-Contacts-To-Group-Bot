

namespace DemoProject
{
    public static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
       
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}