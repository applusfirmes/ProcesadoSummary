using Microsoft.Win32;
using ProcesadoSummary.Utilities;
using ProcesadoSummary.Views;
using System.IO;
using System.Windows.Input;

namespace ProcesadoSummary.ViewModel
{
    internal class NavigationViewModel : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        private string _nombreMdbSelected;
        public string NombreMdbSelected
        {
            get => _nombreMdbSelected;
            set
            {
                _nombreMdbSelected = value;
                OnPropertyChanged(nameof(NombreMdbSelected));
            }
        }


        //VIEWS
        public ICommand GenerarTDCommand { get; set; }
        public ICommand GenerarInformeCommand { get; set; }        
        public ICommand SelectAndImportCommand { get; set; }
        private void GenerarTD(object obj) => CurrentView = new GenerarTDViewModel();
        private void GenerarInforme(object obj) => CurrentView = new GenerarInformeViewModel();
        private void SelectAndImport(object obj) => CurrentView = new SelectAndImportViewModel();

        public NavigationViewModel()
        {
            GenerarTDCommand = new RelayCommand(GenerarTD);
            GenerarInformeCommand = new RelayCommand(GenerarInforme);
            SelectAndImportCommand = new RelayCommand(SelectAndImport);

            // Startup Page
            CurrentView = new SelectAndImportViewModel();
        }

    }
}
