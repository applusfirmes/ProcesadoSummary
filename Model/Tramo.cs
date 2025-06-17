namespace ProcesadoSummary.Model
{
    public class Tramo
    {
        public int Id { get; set; }
        public int IdCarretera { get; set; }
        public string PKI { get; set; }
        public string PKF { get; set; }
        public string Carril { get; set; }
        public string NumTramo { get; set; }
        public string Observaciones { get; set; }
    }
}
