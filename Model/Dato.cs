using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProcesadoSummary.Model
{
    public class Dato
    {
        public int Id { get; set; }
        public int IdTramo { get; set; }
        public int Dist_Origen { get; set; }
        public string PKI { get; set; }
        public string PKF { get; set; }
        public int Archivo { get; set; }
        public string Nombre { get; set; }
        public string Area_total { get; set; }
        public string Long_total { get; set; }
        public string IFTotal { get; set; }
        public string Long_proyec { get; set; }
        public string IFP { get; set; }
        public string Area_long { get; set; }
        public string Long_long { get; set; }
        public string IFL { get; set; }
        public string Area_trans { get; set; }
        public string Long_trans { get; set; }
        public string IFT { get; set; }
        public string Area_otras { get; set; }
        public string Long_otras { get; set; }
        public string IFO { get; set; }
        public string Area_malla { get; set; }
        public string Long_malla { get; set; }
        public string IFM { get; set; }
        public string Prof_r_izq { get; set; }
        public string Ancho_r_izq { get; set; }
        public string Area_ri { get; set; }
        public string Prof_r_der { get; set; }
        public string Ancho_r_der { get; set; }
        public string Area_rd { get; set; }
        public string Textura_b1 { get; set; }
        public string Textura_b2 { get; set; }
        public string Textura_b3 { get; set; }
        public string Textura_b4 { get; set; }
        public string Textura_b5 { get; set; }
        public string Textura { get; set; }
        public string Resul_ravelling { get; set; }
        public string N_baches { get; set; }
        public string Area_baches { get; set; }
        public string Area_parches { get; set; }
        public string Long_parches { get; set; }
        public string Indice_parches { get; set; }
        public string Pos_linea_izq { get; set; }
        public string Pos_linea_der { get; set; }
        public string Ancho_carril { get; set; }
        public string Validar_carril { get; set; }
        public string UTM_X { get; set; }
        public string UTM_Y { get; set; }
        public string UTM_Z { get; set; }
        public string Ancho_maximo { get; set; }
        public string Observaciones { get; set; }


    }
}
