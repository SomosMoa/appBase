using System;
namespace apiBillFold.Externo
{
	public class detalleOrden
	{
		public detalleOrden()
		{
		}
		public string ReferenceOrden {get;set;}


		public string ReferenceProduct { get; set; }


		public int cantidad { get; set; }



		public string nombre { get; set; }



		public decimal precio { get; set; }


		public decimal subtotal { get; set; }

    }
}

