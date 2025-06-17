using System.Data.OleDb;
using System.Windows;

namespace ProcesadoSummary.Utilities
{
    internal class AccessDataService
    {
        private string connectionString;

        public OleDbConnection connection { get; set; }    // conexion al access.

        public AccessDataService(string filePath)
        {
            connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};";
        }

        public void abrirConexion()
        {
            try
            {
                connection = new OleDbConnection(connectionString);
                connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en función abrirConexion : " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
