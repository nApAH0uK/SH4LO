using System;
using System.Windows.Forms;

namespace BHEIIIKA
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Запуск главной формы
            Application.Run(new Form2());
        }
    }
}