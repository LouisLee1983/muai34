using OfficeOpenXml;
using System.Runtime.ConstrainedExecution;

namespace WF_MUAI_34
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            // Correct usage for EPPlus 8 and above
            ExcelPackage.License.SetNonCommercialPersonal("李金远");

            


            Application.Run(new MainForm());
        }
    }
}