using System;
using apiBillFold.Models;

namespace apiBillFold.Externo
{
	public class operaciones
	{
		public operaciones()
		{
		}
		public UsuarioMovimiento crearCredito(long idUsuario, decimal monto, long idCuenta,String usuario, string ubicacion,string descripcion, string documento) {
			UsuarioMovimiento mov = new UsuarioMovimiento();
            mov.IdUsuario = idUsuario;
            mov.Monto = monto;
            mov.Idcuenta = idCuenta;
            mov.Tipo = 1;
            mov.MovContable = 0;
            mov.Usuario = usuario;
            mov.Ubicacion = ubicacion;
            mov.Descripcion = descripcion;
            mov.Documento = documento;
            mov.Fecha = DateTime.Now;
            mov.Sfecha = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            return mov;

		}
        public UsuarioMovimiento crearDebito(long idUsuario, decimal monto, long idCuenta, String usuario, string ubicacion, string descripcion, string documento)
        {
            UsuarioMovimiento mov = new UsuarioMovimiento();
            mov.IdUsuario = idUsuario;
            mov.Monto = monto;
            mov.Idcuenta = idCuenta;
            mov.Tipo = 0;
            mov.MovContable = 0;
            mov.Usuario = usuario;
            mov.Ubicacion = ubicacion;
            mov.Descripcion = descripcion;
            mov.Documento = documento;
            mov.Fecha = DateTime.Now;
            mov.Sfecha = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            return mov;
            

        }
    }
}

