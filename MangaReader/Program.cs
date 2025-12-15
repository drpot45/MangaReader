using System;
using System.Windows.Forms;

namespace MangaReader
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize database
            try
            {
                new Models.MangaContext();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}");
                return;
            }

            Application.Run(new LoginForm());
        }
    }
}