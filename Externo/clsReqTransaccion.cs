using System;
namespace apiBillFold.Controllers
{
	public class clsReqTransaccion
	{
		public clsReqTransaccion()
		{
		}

		public string usuario { get; set; }
		public decimal monto { get; set; }
		public string idPromocion { get; set; }
		public string idFormaPago { get; set; }
		public int tipoTransaccion { get; set; }
		public string descripcion { get; set; }
		public int strEmpresa { get; set; }
		public string documento { get; set; }
	}
}

