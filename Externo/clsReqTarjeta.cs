using System;
using System.ComponentModel.DataAnnotations;

namespace apiBillFold.Externo
{
    public class clsReqTarjeta
    {
        public clsReqTarjeta()
        {
        }
        public long id { get; set; }
        public long? idUsuario { get; set; }
        public string tarjeta { get; set; }
        [StringLength(150)]
        public string nombre { get; set; }
        [StringLength(150)]
        public string apellido { get; set; }
        [StringLength(25)]
        public string tipoTarjeta { get; set; }
        [StringLength(10)]
        public string fechaExpiracion { get; set; }
        [StringLength(150)]
        public string cvv { get; set; }
        public long? direccionID { get; set; }
        public string hash { get; set; }
    }
}

