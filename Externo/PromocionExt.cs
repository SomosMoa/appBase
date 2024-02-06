using System;
namespace apiBillFold.Externo
{
    public class PromocionExt
    {
        public PromocionExt(string _id, string _nombre, string _descripcion)
        {
            nombre = _nombre;
            id = _id;
            descripcion = _descripcion;
        }
        public string nombre { get; set; }
        public string id { get; set; }
        public string descripcion { get; set; }

    }
}

