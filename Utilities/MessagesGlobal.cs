using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProcesadoSummary.Utilities
{
    public static class MessagesGlobal
    {
        public static void MessageError(string msj, string titulo = "")
        {
            MessageBox.Show(msj, titulo, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void MessageInfo(string msj, string titulo = "")
        {
            MessageBox.Show(msj, titulo, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void MessageWarning(string msj, string titulo = "")
        {
            MessageBox.Show(msj, titulo, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
