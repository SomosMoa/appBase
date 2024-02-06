using System;
using apiBillFold.Models;

namespace apiBillFold.Externo
{
	public class clsTransferencia
	{
		public clsTransferencia()
		{
		}
		public Campana encabezado { get; set; }
		public List<TransaccionesCampana> detalle  {get;set;}
	}
}

