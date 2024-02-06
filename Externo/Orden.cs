using System;
namespace apiBillFold.Externo
{
    public class Orden
    {
        public Orden()
        {
        }
        public decimal monto { get; set; }
        public double numero { get; set; }
        public string estado { get; set; }
        public string usuario { get; set; }
        public string vendedor { get; set; }
        public string id { get; set; }
        public string direccion { get; set; }
        public string fecha { get; set; }

    }
}

