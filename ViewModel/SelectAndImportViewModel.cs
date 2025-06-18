using Microsoft.Win32;
using OfficeOpenXml;
using ProcesadoSummary.Model;
using ProcesadoSummary.Repositorio;
using ProcesadoSummary.Utilities;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Security.RightsManagement;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace ProcesadoSummary.ViewModel
{
    class SelectAndImportViewModel : ViewModelBase
    {

        //VARIABLES
        private Consultas consultas;

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

        private string _txtInicio;
        public string txtInicio
        {
            get => _txtInicio;
            set
            {
                _txtInicio = value;
                OnPropertyChanged(nameof(txtInicio));
            }
        }


        private string _txtFin;
        public string txtFin
        {
            get => _txtFin;
            set
            {
                _txtFin = value;
                OnPropertyChanged(nameof(txtFin));
            }
        }

        //---COMBOBOX----//

        public ObservableCollection<int> ComboboxMetros { get; set; }
        private int _metrosSelected;
        public int MetrosSelected
        {
            get => _metrosSelected;
            set
            {
                _metrosSelected = value;
                OnPropertyChanged(nameof(MetrosSelected));
            }
        }

        public ICommand SelectMdbCommand { get; set; }
        public ICommand ImportSummaryCommand { get; set; }

        public SelectAndImportViewModel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("<Sebastian>");

            ComboboxMetros = new ObservableCollection<int>();

            SelectMdbCommand = new RelayCommand(OnClickBtnSelectMdb);
            ImportSummaryCommand = new RelayCommand(OnClickImportSummary);

            LoadComboboxMetros();
        }

        private void LoadComboboxMetros()
        {
            ComboboxMetros.Clear();
            ComboboxMetros.Add(10);
            ComboboxMetros.Add(20);
            ComboboxMetros.Add(100);
        }

        private void OnClickBtnSelectMdb(object obj)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Seleccionar archivo MDB";
                openFileDialog.Filter = "Database Files | *.accdb";

                if (openFileDialog.ShowDialog() == true)
                {
                    //Si selecciona archivo
                    //rutaArchivoMdb = openFileDialog.FileName;
                    string rutaMdb = openFileDialog.FileName;

                    ConexionConsultas(rutaMdb);
                    NombreMdbSelected = Path.GetFileName(rutaMdb); //Obtenemos solo el nombre de la MDB
                }
            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error función OnClickBtnSelectMdb: {ex.Message}");
            }

        }

        //Importar ficheros summary a la MDB
        private void OnClickImportSummary(object obj)
        {
            try
            {
                if (string.IsNullOrEmpty(NombreMdbSelected))
                {
                    MessagesGlobal.MessageWarning("Seleccione un archivo MDB.", "MDB");

                    return;
                }

                bool ok = ValidarDatosEntrada();
                if (ok)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Title = "Seleccionar archivo xlsx";
                    openFileDialog.Filter = "Excel Files | *.xlsx";

                    if (openFileDialog.ShowDialog() == true)
                    {
                        string rutaFicheroSummary = openFileDialog.FileName;
                        string nombreFicheroSummary = Path.GetFileName(rutaFicheroSummary);

                        //Obtenemos la carretera del nombre del fichero Summary
                        string carretera = GetCarreteraSummary(nombreFicheroSummary);

                        if (!string.IsNullOrEmpty(carretera))
                        {
                            //Comprobamos si existe o no en la MDB para evitar duplicados
                            Carretera c = consultas.GetCarreteraByName(carretera);
                            if (c == null) //Si NO existe, lo creamos
                            {
                                int idCarretera = CreateAndAddCarretera(carretera);

                                //Si hemos insertado correctamente y obtenido un ID valido creamos un TRAMO
                                if (idCarretera != 0)
                                {
                                    string carril = GetCarrilSummary(nombreFicheroSummary);
                                    string numTramo = GetTramoSummary(nombreFicheroSummary);

                                    int idTramo = CreateAndAddTramo(idCarretera, carril, numTramo);

                                    //Si hemos insertado correctamente y obtenido un ID valido creamos un DATOS
                                    if (idTramo != 0)
                                    {
                                        //Obtenemos lista de datos de las filas leidas del Summary
                                        var listaDatos = ReadSummary(rutaFicheroSummary, idTramo);

                                        //Si la lista esta completa, insertamos en MDB
                                        if (listaDatos.Count > 0)
                                        {
                                            InsertarDatos(listaDatos);

                                            //Después de insertar datos, generamos ficheros EXCEL 'TABLAS CADA 10M'
                                            GenerarTablasCada10m(listaDatos, nombreFicheroSummary, rutaFicheroSummary);
                                            MessagesGlobal.MessageInfo("Datos guardados y exportados.");
                                            ClearTxtFields();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //Si ya existe la carretera no la creamos
                                //y comprobamos si existe el tramo o no
                                string carril = GetCarrilSummary(nombreFicheroSummary);
                                string numTramo = GetTramoSummary(nombreFicheroSummary);
                                Tramo t = consultas.GetTramoByCarreteraAndCarril(c.Id, carril);

                                if (t == null) //Si NO existe, lo creamos
                                {
                                    int idTramo = CreateAndAddTramo(c.Id, carril, numTramo);

                                    //Si hemos insertado correctamente y obtenido un ID valido creamos un DATOS
                                    if (idTramo != 0)
                                    {
                                        //Obtenemos lista de datos de las filas leidas del Summary
                                        var listaDatos = ReadSummary(rutaFicheroSummary, idTramo);

                                        //Si la lista esta completa, insertamos en MDB
                                        if (listaDatos.Count > 0)
                                        {
                                            InsertarDatos(listaDatos);

                                            //Después de insertar datos, generamos ficheros EXCEL 'TABLAS CADA 10M'
                                            GenerarTablasCada10m(listaDatos, nombreFicheroSummary, rutaFicheroSummary);
                                            MessagesGlobal.MessageInfo("Datos guardados y exportados.");
                                            ClearTxtFields();
                                        }
                                    }
                                }
                                else
                                {
                                    MessagesGlobal.MessageWarning("Summary ya importado.");
                                }
                            }
                        }

                    }
                }
                else
                {
                    MessagesGlobal.MessageWarning($"Los datos no pueden estar vacíos.");
                }
            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error función OnClickImportSummary: {ex.Message}");
            }
        }

        private void ClearTxtFields()
        {
            txtInicio = string.Empty;
            txtFin = string.Empty;
        }

        private string GetTramoSummary(string nombreFicheroSummary)
        {
            string numTramo = "";

            try
            {
                string[] partes = nombreFicheroSummary.Split('_');

                if (partes.Length == 5)
                {
                    numTramo = partes[3]; //V1
                    return numTramo;
                }
            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error GetTramoSummary: {ex.Message}");
            }

            return numTramo;
        }

        private string GetCarrilSummary(string nombreFicheroSummary)
        {
            string carril = "";

            try
            {
                string[] partes = nombreFicheroSummary.Split('_');

                if (partes.Length == 5)
                {
                    carril = partes[2]; //V1
                    return carril;
                }
            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error GetCarrilSummary: {ex.Message}");
            }

            return carril;
        }

        private int CreateAndAddTramo(int idCarretera, string carril, string numTramo)
        {
            try
            {
                Tramo t = new Tramo
                {
                    IdCarretera = idCarretera,
                    PKI = "",
                    PKF = "",
                    Carril = carril,
                    NumTramo = numTramo,
                    Observaciones = ""
                };

                return consultas.AddTramo(t);

            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error al CreateAndAddTramo: {ex.Message}");
                return 0;
            }
        }

        private int CreateAndAddCarretera(string carretera)
        {
            try
            {
                Carretera c = new Carretera
                {
                    Nombre = carretera
                };

                return consultas.AddCarretera(c);

            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error al CreateAndAddCarretera: {ex.Message}");
                return 0;
            }
        }

        private string GetCarreteraSummary(string nombreFicheroSummary)
        {
            string nombreCarretera = "";

            try
            {
                string[] partes = nombreFicheroSummary.Split('_');

                if (partes.Length == 5)
                {
                    nombreCarretera = partes[1];
                    return nombreCarretera;
                }
            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error al obtener GetCarreteraSummary: {ex.Message}");
            }

            return nombreCarretera;
        }

        private void GenerarTablasCada10m(List<Dato> listaDatos, string nombreFicheroSummary, string rutaFicheroSummary)
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Datos");

                    // Definir cabeceras fijas
                    string[] cabeceras = new string[]{
                        "Dist. Origen(m)", "PK inicial", "PK final", "Archivo", "Nombre", "Área Total(m2)", "Long. Total(m)", "IFTotal",
                        "Long. Proyec.(m)", "IFP", "Área Long.(m2)", "Long. Long.(m)", "IFL", "Área Trans.(m2)", "Long. Trans.(m)", "IFT",
                        "Área Otras(m2)", "Long. Otras(m)", "IFO", "Área Malla(m2)", "Long. Malla(m)", "IFM",
                        "Prof. R.Izq.(mm)", "Ancho R.Izq.(mm)", "Área R.I.(mm2)", "Prof. R.Der.(mm)", "Ancho R.Der.(mm)", "Área R.D.(mm2)",
                        "Textura B.1", "Textura B.2", "Textura B.3", "Textura B.4", "Textura B.5", "Textura",
                        "Resul. Raveling", "Nº baches", "Área baches", "Área parches", "Long. parches", "Índice parches",
                        "Pos. línea izq.(mm)", "Pos. línea der.(mm)", "Ancho carril(mm)", "Validar carril(mm)",
                        "UTM_X", "UTM_Y", "UTM_Z", "Ancho máximo", "Observaciones"};

                    //Escribimos cabecera en fila 1
                    for (int i = 0; i < cabeceras.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = cabeceras[i];
                    }

                    // Aplicar formato a la cabecera
                    var cabeceraRango = worksheet.Cells[1, 1, 1, cabeceras.Length];
                    cabeceraRango.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    cabeceraRango.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(250, 191, 143));
                    cabeceraRango.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    cabeceraRango.Style.Font.Bold = true;

                    // Ajustar ancho automáticamente
                    worksheet.Cells[1, 1, 1, cabeceras.Length].AutoFitColumns();

                    // Insertar datos
                    for (int i = 0; i < listaDatos.Count; i++)
                    {
                        var dato = listaDatos[i];
                        worksheet.Cells[i + 2, 1].Value = dato.Dist_Origen;
                        worksheet.Cells[i + 2, 2].Value = dato.PKI;
                        worksheet.Cells[i + 2, 3].Value = dato.PKF;
                        worksheet.Cells[i + 2, 4].Value = dato.Archivo;
                        worksheet.Cells[i + 2, 5].Value = dato.Nombre;
                        worksheet.Cells[i + 2, 6].Value = dato.Area_total;
                        worksheet.Cells[i + 2, 7].Value = dato.Long_total;
                        worksheet.Cells[i + 2, 8].Value = dato.IFTotal;
                        worksheet.Cells[i + 2, 9].Value = dato.Long_proyec;
                        worksheet.Cells[i + 2, 10].Value = dato.IFP;
                        worksheet.Cells[i + 2, 11].Value = dato.Area_long;
                        worksheet.Cells[i + 2, 12].Value = dato.Long_long;
                        worksheet.Cells[i + 2, 13].Value = dato.IFL;
                        worksheet.Cells[i + 2, 14].Value = dato.Area_trans;
                        worksheet.Cells[i + 2, 15].Value = dato.Long_trans;
                        worksheet.Cells[i + 2, 16].Value = dato.IFT;
                        worksheet.Cells[i + 2, 17].Value = dato.Area_otras;
                        worksheet.Cells[i + 2, 18].Value = dato.Long_otras;
                        worksheet.Cells[i + 2, 19].Value = dato.IFO;
                        worksheet.Cells[i + 2, 20].Value = dato.Area_malla;
                        worksheet.Cells[i + 2, 21].Value = dato.Long_malla;
                        worksheet.Cells[i + 2, 22].Value = dato.IFM;
                        worksheet.Cells[i + 2, 23].Value = dato.Prof_r_izq;
                        worksheet.Cells[i + 2, 24].Value = dato.Ancho_r_izq;
                        worksheet.Cells[i + 2, 25].Value = dato.Area_ri;
                        worksheet.Cells[i + 2, 26].Value = dato.Prof_r_der;
                        worksheet.Cells[i + 2, 27].Value = dato.Ancho_r_der;
                        worksheet.Cells[i + 2, 28].Value = dato.Area_rd;
                        worksheet.Cells[i + 2, 29].Value = dato.Textura_b1;
                        worksheet.Cells[i + 2, 30].Value = dato.Textura_b2;
                        worksheet.Cells[i + 2, 31].Value = dato.Textura_b3;
                        worksheet.Cells[i + 2, 32].Value = dato.Textura_b4;
                        worksheet.Cells[i + 2, 33].Value = dato.Textura_b5;
                        worksheet.Cells[i + 2, 34].Value = dato.Textura;
                        worksheet.Cells[i + 2, 35].Value = dato.Resul_ravelling;
                        worksheet.Cells[i + 2, 36].Value = dato.N_baches;
                        worksheet.Cells[i + 2, 37].Value = dato.Area_baches;
                        worksheet.Cells[i + 2, 38].Value = dato.Area_parches;
                        worksheet.Cells[i + 2, 39].Value = dato.Long_parches;
                        worksheet.Cells[i + 2, 40].Value = dato.Indice_parches;
                        worksheet.Cells[i + 2, 41].Value = dato.Pos_linea_izq;
                        worksheet.Cells[i + 2, 42].Value = dato.Pos_linea_der;
                        worksheet.Cells[i + 2, 43].Value = dato.Ancho_carril;
                        worksheet.Cells[i + 2, 44].Value = dato.Validar_carril;
                        worksheet.Cells[i + 2, 45].Value = dato.UTM_X;
                        worksheet.Cells[i + 2, 46].Value = dato.UTM_Y;
                        worksheet.Cells[i + 2, 47].Value = dato.UTM_Z;
                        worksheet.Cells[i + 2, 48].Value = dato.Ancho_maximo;
                        worksheet.Cells[i + 2, 49].Value = dato.Observaciones;
                    }

                    // Auto-ajustar columnas
                    worksheet.Cells.AutoFitColumns();

                    // Guardar el archivo
                    string nombreTD = nombreFicheroSummary.Replace("Summary", "TD");
                    string rutaDestino = Path.GetDirectoryName(rutaFicheroSummary);
                    var path = Path.Combine(rutaDestino, nombreTD);
                    File.WriteAllBytes(path, package.GetAsByteArray());
                }
            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error en función GenerarTablasCada10m: {ex.Message}");
            }
        }

        private bool ValidarDatosEntrada()
        {
            if (string.IsNullOrEmpty(_txtFin) || string.IsNullOrEmpty(_txtInicio))
            {
                return false;
            }

            return true;
        }

        private void InsertarDatos(List<Dato> listaDatos)
        {
            try
            {
                foreach (var d in listaDatos)
                {
                    consultas.AddDato(d);
                }
            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error en función InsertarDatos: {ex.Message}");
            }
        }

        private List<Dato> ReadSummary(string rutaFicheroSummary, int idTramo)
        {
            var listaDatos = new List<Dato>();

            try
            {

                using (var package = new ExcelPackage(new FileInfo(rutaFicheroSummary)))
                {
                    ExcelWorksheet hoja = package.Workbook.Worksheets[0]; // Primera hoja

                    int filas = hoja.Dimension.End.Row;
                    int columnas = hoja.Dimension.End.Column;

                    int inicio = Convert.ToInt32(_txtInicio);
                    int fin = Convert.ToInt32(_txtFin);

                    int contDist = 0;
                    int valorSaltar = 0;
                    if (MetrosSelected == 20)
                    {
                        valorSaltar = 1;
                    }
                    else if (MetrosSelected == 100)
                    {
                        valorSaltar = 9;
                    }

                    //Por cada fila que leemos creamos un objeto 'Dato' que guaradremos en 'listaDatos'
                    for (int fila = 2; fila <= filas; fila++) // Saltamos la cabecera
                    {
                        try
                        {
                            string nomImage = hoja.Cells[fila, 12].Text; // Columna L - FileName
                            Match match = Regex.Match(nomImage, @"LcmsResult_(\d+)\.xml");
                            if (match.Success)
                            {
                                int numero = int.Parse(match.Groups[1].Value);

                                //Si estoy dentro del rango que define el usuario, me guardare esos datos
                                if (numero >= inicio && numero <= fin)
                                {
                                    float valorPothholesArea = float.Parse(hoja.Cells[fila, 55].Text, new CultureInfo("es-ES"));

                                    int valorNBaches = Convert.ToInt32(hoja.Cells[fila, 54].Text); // Columna BB - NbPotholes
                                    string NBaches = valorNBaches.ToString();
                                    string areaBaches = "0";
                                    //Leemos la columna 54 para saber si hay que sumar valores a algunas columnas
                                    int valorPolishing = 0; // Columna BI - PolishingSection
                                    if (!string.IsNullOrEmpty(hoja.Cells[fila, 54].Text))
                                    {
                                        valorPolishing = Convert.ToInt32(hoja.Cells[fila, 54].Text);
                                        if (valorPolishing > 0)
                                        {
                                            string input = Microsoft.VisualBasic.Interaction.InputBox($"Cuántos baches tiene la sección {nomImage} ?", "Entrada requerida", "0");

                                            int valorUsuario = 0;
                                            if (int.TryParse(input, out valorUsuario))
                                            {
                                                // usar valorUsuario
                                                NBaches = (valorNBaches + valorUsuario).ToString(); // Columna BB - NbPotholes

                                                areaBaches = ((valorPothholesArea / 1000000) + valorPolishing).ToString(); // Columna BC - PotholesArea
                                            }
                                        }
                                    }


                                    // 1 Columna A
                                    // 2 Columna B
                                    //int distOrigen = int.Parse(hoja.Cells[fila, 3].Text);  // Columna C - DistBegin
                                    int distOrigen = contDist;  // Columna C - DistBegin
                                    string UTMX = hoja.Cells[fila, 9].Text; // Columna I - UTMX
                                    string UTMY = hoja.Cells[fila, 10].Text; // Columna J - UTMY
                                    string UTMZ = hoja.Cells[fila, 11].Text; // Columna K - UTMZ
                                    string nombre = hoja.Cells[fila, 12].Text; // Columna L - FileName
                                    string longTotal = hoja.Cells[fila, 13].Text; // Columna M - TotalLong
                                    string areaTotal = hoja.Cells[fila, 14].Text; // Columna N - TotalArea
                                    string longLong = hoja.Cells[fila, 15].Text; // Columna O - LongLong
                                    string areaLong = hoja.Cells[fila, 16].Text; // Columna P - LongArea
                                    string longTrans = hoja.Cells[fila, 17].Text; // Columna Q - TransLong
                                    string areaTrans = hoja.Cells[fila, 18].Text; // Columna R - TransArea
                                    string longMalla = hoja.Cells[fila, 19].Text; // Columna S - AlligatorLong
                                    string areaMalla = hoja.Cells[fila, 20].Text; // Columna T - AlligatorArea
                                    string longOtras = hoja.Cells[fila, 21].Text; // Columna U - OthersLong
                                    string areaOtras = hoja.Cells[fila, 22].Text; // Columna V - OthersArea
                                    string longProyec = hoja.Cells[fila, 23].Text; // Columna W - Projected
                                    string iftTotal = hoja.Cells[fila, 28].Text; // Columna AB - IFT
                                    string IFL = hoja.Cells[fila, 29].Text; // Columna AC - IFL
                                    string IFT = hoja.Cells[fila, 30].Text; // Columna AD - IFTR
                                    string IFM = hoja.Cells[fila, 31].Text; // Columna AE - IFM
                                    string IFO = hoja.Cells[fila, 32].Text; // Columna AF - IFO
                                    string IFP = hoja.Cells[fila, 33].Text; // Columna AG - IFP
                                    string profRIzq = hoja.Cells[fila, 42].Text; // Columna AP - LeftRut
                                    string anchoRIzq = hoja.Cells[fila, 43].Text; // Columna AQ - LeftRut
                                    string areaRI = hoja.Cells[fila, 44].Text; // Columna AR - LeftWheelPathSection
                                    string profRDer = hoja.Cells[fila, 45].Text; // Columna AS - RightRut
                                    string anchoRDer = hoja.Cells[fila, 46].Text; // Columna AT - RightWheelPathWidth
                                    string areaRD = hoja.Cells[fila, 47].Text; // Columna AU - RightWheelPathSection

                                    string texturaB1 = string.IsNullOrWhiteSpace(hoja.Cells[fila, 48].Text) ? "0" : hoja.Cells[fila, 48].Text; // Columna AV - Textura B1
                                    string texturaB2 = string.IsNullOrWhiteSpace(hoja.Cells[fila, 49].Text) ? "0" : hoja.Cells[fila, 49].Text; // Columna AW - Textura B2
                                    string texturaB3 = string.IsNullOrWhiteSpace(hoja.Cells[fila, 50].Text) ? "0" : hoja.Cells[fila, 50].Text; // Columna AX - Textura B3
                                    string texturaB4 = string.IsNullOrWhiteSpace(hoja.Cells[fila, 51].Text) ? "0" : hoja.Cells[fila, 51].Text; // Columna AY - Textura B4
                                    string texturaB5 = string.IsNullOrWhiteSpace(hoja.Cells[fila, 52].Text) ? "0" : hoja.Cells[fila, 52].Text; // Columna AZ - Textura B5
                                    string textura = string.IsNullOrWhiteSpace(hoja.Cells[fila, 53].Text) ? "0" : hoja.Cells[fila, 53].Text; // Columna BA - AverageTextura
                                    string resulRaveling = string.IsNullOrWhiteSpace(hoja.Cells[fila, 65].Text) ? "0" : hoja.Cells[fila, 65].Text; // Columna BM - Raveling                              

                                    string areaParches = "0";
                                    string longParches = "0";
                                    string indiceParches = "0";

                                    string anchoCarril = hoja.Cells[fila, 56].Text; // Columna BD - WidthLane

                                    string validarCarril = Convert.ToInt32(anchoCarril) >= 2400 ? "VERDADERO" : "FALSO"; //Si el anchoCarril es mayor o igual que 2400, guardaremos VERDADERO, de lo contrario FALSO.

                                    string posLinIzq = hoja.Cells[fila, 66].Text; // Columna BN - LeftLaneMark
                                    string posLinDer = hoja.Cells[fila, 67].Text; // Columna BO - RightLaneMark
                                    string anchoMaximo = hoja.Cells[fila, 68].Text; // Columna BP - MaxCrackWidth

                                    string PKI = hoja.Cells[fila, 5].Text + "+" + hoja.Cells[fila, 6].Text;

                                    //IMPORTANTE LA VARIABLE valorSaltar sumado ala fila para recogerel final PKF correspondiente
                                    string PKF = hoja.Cells[fila, 7].Text + "+" + hoja.Cells[fila + valorSaltar, 8].Text;

                                    //Si es la útlima fila....
                                    if (numero == fin)
                                    {
                                        int sumar = 0;
                                        if (valorSaltar == 1)
                                        {
                                            sumar = 10;
                                        }
                                        else if (valorSaltar == 9)
                                        {
                                            sumar = 90;
                                        }

                                        int mFin = Convert.ToInt32(hoja.Cells[fila, 8].Text) + sumar;

                                        PKF = hoja.Cells[fila, 7].Text + "+" + mFin.ToString();
                                    }

                                    string pkiMod = ModSintaxisPKM(PKI);
                                    string pkfMod = ModSintaxisPKM(PKF);

                                    var dato = new Dato
                                    {
                                        IdTramo = idTramo,
                                        Dist_Origen = distOrigen,
                                        PKI = pkiMod,
                                        PKF = pkfMod,
                                        Archivo = numero,
                                        Nombre = nombre,
                                        Area_total = areaTotal,
                                        Long_total = longTotal,
                                        IFTotal = iftTotal,
                                        Long_proyec = longProyec,
                                        IFP = IFP,
                                        Area_long = areaLong,
                                        Long_long = longLong,
                                        IFL = IFL,
                                        Area_trans = areaTrans,
                                        Long_trans = longTrans,
                                        IFT = IFT,
                                        Area_otras = areaOtras,
                                        Long_otras = longOtras,
                                        IFO = IFO,
                                        Prof_r_izq = profRIzq,
                                        Ancho_r_izq = anchoRIzq,
                                        Area_ri = areaRI,
                                        Prof_r_der = profRDer,
                                        Ancho_r_der = anchoRDer,
                                        Area_rd = areaRD,
                                        Pos_linea_izq = posLinIzq,
                                        Pos_linea_der = posLinDer,
                                        Ancho_carril = anchoCarril,
                                        UTM_X = UTMX,
                                        UTM_Y = UTMY,
                                        UTM_Z = UTMZ,
                                        Ancho_maximo = anchoMaximo,
                                        Area_malla = areaMalla,
                                        Long_malla = longMalla,
                                        Long_parches = longParches,
                                        IFM = IFM,
                                        Textura = textura,
                                        Textura_b1 = texturaB1,
                                        Textura_b2 = texturaB2,
                                        Textura_b3 = texturaB3,
                                        Textura_b4 = texturaB4,
                                        Textura_b5 = texturaB5,
                                        Resul_ravelling = resulRaveling,
                                        N_baches = NBaches,
                                        Area_baches = areaBaches,
                                        Area_parches = areaParches,
                                        Indice_parches = indiceParches,
                                        Validar_carril = validarCarril
                                    };

                                    if (valorSaltar == 1)
                                    {
                                        contDist += 20;
                                    }
                                    else if (valorSaltar == 9)
                                    {
                                        contDist += 100;
                                    }
                                    else
                                    {
                                        contDist += 10;
                                    }


                                    listaDatos.Add(dato);
                                    fila += valorSaltar;

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessagesGlobal.MessageError($"Error al crear dato, fila {fila}: {ex.Message}");
                            //Cuando usuario le de a aceptar, continuará el bucle
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error función ReadSummary: {ex.Message}");
            }

            return listaDatos;

        }

        private string ModSintaxisPKM(string pkm)
        {
            string[] array = pkm.Split("+");
            string PK = array[0];
            string metros = array[1];

            switch (metros.Length)
            {
                case 1:
                    metros = "00" + metros;
                    break;
                case 2:
                    metros = "0" + metros;
                    break;
                default:

                    break;
            }

            string PKMCorregido = PK + "+" + metros;

            return PKMCorregido;
        }

        private void ConexionConsultas(string rutaArchivo)
        {
            try
            {
                //Abrimos conexion
                AccessDataService accessDataService = new AccessDataService(rutaArchivo);
                accessDataService.abrirConexion();

                //Creamos objeto consultasBBDD, que es donde tenemos la logica de la recogida de datos
                consultas = new Consultas(accessDataService.connection);
            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error en función conexionConsultasBBDD:{ex.Message}");
            }
        }

    }
}
