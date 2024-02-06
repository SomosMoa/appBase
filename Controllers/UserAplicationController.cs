using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using apiBillFold.Externo;
using apiBillFold.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace apiBillFold.Controllers
{
    [Route("api/[controller]")]
    public class UserAplicationController : Controller
    {
        private readonly BilleteraCMContext _context;
        private readonly BilleteraCMContextProcedures _contextSP;

        public IConfiguration _configuration;

        public UserAplicationController(BilleteraCMContext context, IConfiguration configuration)
        {
            _context = context;
            _contextSP = new BilleteraCMContextProcedures(_context);
            _configuration = configuration;
        }

        private string limpiar(String texto)
        {
            if (texto.Equals("empty"))
            {
                return "";
            }
            return texto;
        }
        [HttpGet("validarEmpresaCliente")]
        public async Task<clsRespuesta> validarEmpresaAsociada(String strMail) {
            clsRespuesta respuesta = new clsRespuesta();
            List<Usuario> socio = _context.Usuario.Where(x => x.Alias.ToUpper() == strMail.ToUpper()).ToList();
            if (socio.Count == 0) {
                respuesta.data = _context.Configuracion.Where(x => x.Llave == "EMPRESAGENERAL").FirstOrDefault().Valor;

            }
            else {
                respuesta.data = socio.FirstOrDefault().IdEmpresa;
            }
            respuesta.resultado = true;
            return respuesta;

        }


        [HttpGet("spAlmacenarUsuario")]
        public async Task<clsRespuesta> agregarUsuario(string primerNombre, string primerApellido, string NIT, string DPI, string idEmpresa, string hash, string alias)
        {
            clsRespuesta respuesta = new clsRespuesta();

            NIT = limpiar(NIT);
            //validaciones
            int ExisteUsuario = _context.Usuario.Where(x => x.Alias.ToUpper() == alias.ToUpper()).Where(x=>x.IdEmpresa.ToString().Equals(idEmpresa)).ToList().Count();
            try
            {
                if (ExisteUsuario == 0)
                {
                    OutputParameter<long?> idEmpleado = new OutputParameter<long?>();
                    Usuario nuevoUsuario = new Usuario();
                    nuevoUsuario.Alias = alias;
                    nuevoUsuario.ApellidoCasada = "";
                    nuevoUsuario.Codigoempleado = "0";
                    nuevoUsuario.Contrasena = null;
                    nuevoUsuario.Disponible = 0;
                    nuevoUsuario.Dpi = DPI;
                    nuevoUsuario.Estado = 1;
                    nuevoUsuario.FechaNacimiento = null;
                    nuevoUsuario.IdEmpresa = long.Parse(idEmpresa);
                    nuevoUsuario.Nit = NIT;
                    nuevoUsuario.OtrosNombres = "";
                    nuevoUsuario.PrimerNombre = primerNombre;
                    nuevoUsuario.PrimerApellido = primerApellido;
                    nuevoUsuario.Salario = 0;
                    nuevoUsuario.SegundoApellido = "";
                    nuevoUsuario.SegundoNombre = "";
                    var creado = _context.Usuario.Add(nuevoUsuario);
                    await _context.SaveChangesAsync();
                    //crear usuario Externo
                    UsuarioExterno extNuevo = new UsuarioExterno();
                    extNuevo.Nombres = primerNombre;
                    extNuevo.Apellidos = primerApellido;
                    extNuevo.Correo = alias;
                    extNuevo.Foto = null;
                    extNuevo.Hash = hash;
                    extNuevo.IdUsuario = nuevoUsuario.Id;
                    extNuevo.Politica = 0;
                    extNuevo.Telefono = 0;
                    var nuevo = _context.UsuarioExterno.Add(extNuevo);
                    UsuarioMail mails = new UsuarioMail();
                    mails.IdUsuario = Int32.Parse(nuevoUsuario.Id.ToString());
                    mails.Encabezado = "";
                    mails.Servidor = "";
                    mails.Completo = alias;
                    mails.Orden = 1;
                    var mail = _context.UsuarioMail.Add(mails);
                    UsuarioCuenta usrCuenta = new UsuarioCuenta();
                    usrCuenta.Llave = "CORRIENTE";
                    usrCuenta.Descripcion = "Cuenta Corriente";
                    usrCuenta.Fecha = DateTime.Now;
                    usrCuenta.IdUsuario = nuevoUsuario.Id;
                    usrCuenta.Saldo = 0;
                    usrCuenta.Usuario = "AppPulli";
                    UsuarioCuenta usrCuenta1 = new UsuarioCuenta();
                    usrCuenta1.Llave = "SORTEO";
                    usrCuenta1.Descripcion = "Cuenta Sorteo";
                    usrCuenta1.Fecha = DateTime.Now;
                    usrCuenta1.IdUsuario = nuevoUsuario.Id;
                    usrCuenta1.Saldo = 0;
                    usrCuenta1.Usuario = "AppPulli";
                    UsuarioCuenta usrCuenta2 = new UsuarioCuenta();
                    usrCuenta2.Llave = "PUNTOS";
                    usrCuenta2.Descripcion = "Cuenta Puntos";
                    usrCuenta2.Fecha = DateTime.Now;
                    usrCuenta2.IdUsuario = nuevoUsuario.Id;
                    usrCuenta2.Saldo = 0;
                    usrCuenta2.Usuario = "AppPulli";
                    Autorizacion nuevoAutorizacion = new Autorizacion();
                    nuevoAutorizacion.FechaCreacion = DateTime.Now;
                    nuevoAutorizacion.IdUsuario = nuevoUsuario.Id;
                    nuevoAutorizacion.Valor = 0;
                    nuevoAutorizacion.Estado = 0;

                    //Bitacora miBitacora = new Bitacora();
                    //miBitacora.Funcion = "Agregar Usuario";
                    //miBitacora.Idempresa = long.Parse(idEmpresa);
                    //miBitacora.Fecha = DateTime.Now;
                    //var UsuarioActivo = await _context.Administrador.FindAsync(1);
                    //miBitacora.Usuario = UsuarioActivo.Alias;
                    //miBitacora.Descripcion = "Creacion de usuario " + primerNombre + " " + primerApellido;
                    //await _context.Bitacora.AddAsync(miBitacora);
                    await _context.UsuarioCuenta.AddAsync(usrCuenta);
                    await _context.UsuarioCuenta.AddAsync(usrCuenta1);
                    await _context.UsuarioCuenta.AddAsync(usrCuenta2);
                    await _context.Autorizacion.AddAsync(nuevoAutorizacion);
                    await _context.SaveChangesAsync();
                    respuesta.resultado = true;

                }
                else {
                    respuesta.resultado = false;
                }
            }


            catch (DbUpdateConcurrencyException)
            {
                respuesta.resultado = false;

            }

            return respuesta;

        }

        [HttpGet("obtenerPromociones")]
        public async Task<clsRespuesta> obtenerPromociones()
        {
            clsRespuesta respuesta = new clsRespuesta();
            List<Promocion> nueva = await _context.Promocion.ToListAsync();
            List<PromocionExt> externa = new List<PromocionExt>();
            foreach (Promocion p in nueva)
            {
                externa.Add(new PromocionExt(p.Id.ToString(), p.Nombre, p.Descripcion));
            }
            respuesta.data = externa;
            respuesta.resultado = true;
            return respuesta;
        }

        [HttpGet("obtenerTarjetas")]
        public async Task<clsRespuesta> obtenerTarjetas(String hash)
        {
            clsRespuesta respuesta = new clsRespuesta();
            UsuarioExterno ext = await _context.UsuarioExterno.Where(x => x.Hash.Equals(hash)).FirstOrDefaultAsync();
            if (ext == null) {
                respuesta.data = false;
                respuesta.mensaje = "no hay valores";
                return respuesta;
            }
            List<UsuarioTarjeta> nueva = await _context.UsuarioTarjeta.Where(x => x.IdUsuario == ext.IdUsuario).ToListAsync();
            foreach (UsuarioTarjeta t in nueva)
            {
                t.Tarjeta = t.Nombre + " - " + t.Tarjeta.Substring(11);
            }

            respuesta.data = nueva;
            respuesta.resultado = true;
            return respuesta;
        }

        [HttpPost("realizarTransaccion")]
        public async Task<clsRespuesta> realizarTransaccion([FromBody] clsReqTransaccion transaccion) {
            clsRespuesta respuesta = new clsRespuesta();
            UsuarioExterno usr = await _context.UsuarioExterno.Where(x => x.Hash == transaccion.usuario).FirstOrDefaultAsync();
            OutputParameter<long?> d = new OutputParameter<long?>();
            //  var respTransaccion = await _contextSP.realizarTransaccionAsync(transaccion.strEmpresa, transaccion.monto, transaccion.tipoTransaccion, transaccion.documento, transaccion.descripcion, usr.IdUsuario, d);
            if (long.Parse(d.Value.ToString()) >= 0) {
                respuesta.data = d.Value;
                respuesta.resultado = true;
            }
            return respuesta;
        }

        [HttpPost("agregarTarjeta")]
        public async Task<clsRespuesta> agregarTarjeta([FromBody] clsReqTarjeta foo)
        {
            clsRespuesta respuesta = new clsRespuesta();
            UsuarioTarjeta t = new UsuarioTarjeta();
            t.Apellido = foo.apellido;
            t.Cvv = foo.cvv;
            t.DireccionId = foo.direccionID;
            t.FechaExpiracion = foo.fechaExpiracion;

            UsuarioExterno ext = await _context.UsuarioExterno.Where(x => x.Hash.Equals(foo.hash)).FirstOrDefaultAsync();
            t.IdUsuario = ext.IdUsuario;
            t.Nombre = foo.nombre;
            t.Tarjeta = foo.tarjeta;
            t.TipoTarjeta = foo.tipoTarjeta;
            await _context.UsuarioTarjeta.AddAsync(t);

            try
            {
                await _context.SaveChangesAsync();
                respuesta.resultado = true;

            }
            catch (DbUpdateConcurrencyException)
            {
                respuesta.resultado = false;

            }
            return respuesta;
        }

        [HttpPost("cuenta/autenticar/")]
        public async Task<clsRespuesta> autenticarEmpleado(String correo, String password, String idPais)
        {
            clsRespuesta respuesta = new clsRespuesta();
            var usuarioLogueado = await _contextSP.ManejoIngresoEmpleadoAsync(correo, password);
            if (usuarioLogueado.Count == 0)
            {
                respuesta.resultado = false;
                respuesta.mensaje = "Valide sus credenciales";
                respuesta.mensajeTecnico = "Datos ingresados no validos";
            }
            else
            {
                ManejoIngresoEmpleadoResult dataUsuario = usuarioLogueado.FirstOrDefault();
                String token = GenerateJSONWebToken(dataUsuario.Id.ToString(), dataUsuario.alias);
                var data = new { idUsuario = dataUsuario.Id, isVerified = true, idPais = idPais, idempresa = dataUsuario.idEmpresa, jwToken = token, refreshToken = "5" };
                respuesta.resultado = true;
                respuesta.data = data;
            }

            return respuesta;
        }

        private string GenerateJSONWebToken(String IdUsuario, String Alias)
        {
            var jwt = _configuration.GetSection("Jwt").Get<jwConfig>();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                new Claim("id", IdUsuario),
                new Claim("usuario",Alias)
            };
            var token = new JwtSecurityToken(jwt.Issuer,
              jwt.Audience,
              claims,
              expires: DateTime.Now.AddMinutes(10),
              signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("tokeUsuario")]
        public async Task<clsRespuesta> obtenerTokenActivo(String Mail) {
            List<getTokenByMailResult> a = await _contextSP.getTokenByMailAsync(Mail);
            clsRespuesta cls = new clsRespuesta();
            if (a.Count == 0)
            {
                cls.resultado = false;
            }
            else {
                cls.resultado = true;
                String token = "";
                foreach (getTokenByMailResult item in a) {
                    token = item.token;
                }
                cls.data = token;
            }
            return cls;
        }
        [HttpGet("validarUsuario")]
        public async Task<ActionResult<clsRespuesta>> validarUsuario(string usuario)
        {
            clsRespuesta respuesta = new clsRespuesta();
            OutputParameter<int?> validor = new OutputParameter<int?>();
            var a = await _contextSP.validarExistenciaUsuarioAsync(usuario, validor);
            if (validor.Value == 1)
            {
                respuesta.resultado = true;
            }
            else {
                respuesta.resultado = false;
            }
            return respuesta;

        }

        [HttpGet("obtenerPromocionSeleccionada")]
        public async Task<clsRespuesta> obtenerPromocionesSeleccionada(string strPromocion)
        {
            clsRespuesta respuesta = new clsRespuesta();
            List<Promocion> nueva = await _context.Promocion.Where(x => x.Id == Int32.Parse(strPromocion)).ToListAsync();

            respuesta.data = nueva;
            respuesta.resultado = true;
            return respuesta;
        }
        [HttpGet("obtenerSaldos")]
        public async Task<clsRespuesta> obtenerSaldos(string usuario) {
            clsRespuesta respuesta = new clsRespuesta();
            UsuarioExterno usr = await _context.UsuarioExterno.Where(x => x.Hash == usuario).FirstOrDefaultAsync();
            long idUsuarioTransaccion = long.Parse(usr.IdUsuario.ToString());

            List<UsuarioCuenta> cuentas = _context.UsuarioCuenta.Where(x => x.IdUsuario == idUsuarioTransaccion).ToList();
            respuesta.data = cuentas;
            respuesta.resultado = true;
            return respuesta;
        }
        [HttpGet("obtenerMovimientosTransaccionales")]
        public async Task<clsRespuesta> obtenerSaldos(string usuario, string tipo, int limit)
        {
            clsRespuesta respuesta = new clsRespuesta();
            UsuarioExterno usr = await _context.UsuarioExterno.Where(x => x.Hash == usuario).FirstOrDefaultAsync();
            long idUsuarioTransaccion = long.Parse(usr.IdUsuario.ToString());
            UsuarioCuenta cuentas = _context.UsuarioCuenta.Where(x => x.IdUsuario == idUsuarioTransaccion).Where(x => x.Llave == tipo).FirstOrDefault();
            List<UsuarioMovimiento> movimientos = movimientos = _context.UsuarioMovimiento.Where(x => x.IdUsuario == idUsuarioTransaccion).Where(x => x.Idcuenta == cuentas.Id).OrderByDescending(x => x.Fecha).ToList();
            if (limit == 0)
            {
                respuesta.data = movimientos;
            }
            else {

                if (limit >= movimientos.Count)
                {
                    respuesta.data = movimientos;
                }
                else {
                    respuesta.data = movimientos.GetRange(0, limit);
                }
            }

            respuesta.resultado = true;
            return respuesta;
        }

        [HttpPost("recargaTarjeta")]
        public async Task<clsRespuesta> RecargaPorTarjeta([FromBody] clsReqTransaccion t) {
            UsuarioMovimiento mov = new UsuarioMovimiento();
            clsRespuesta respuesta = new clsRespuesta();
            UsuarioExterno usr = await _context.UsuarioExterno.Where(x => x.Hash == t.usuario).FirstOrDefaultAsync();
            long idUsuarioTransaccion = long.Parse(usr.IdUsuario.ToString());
            UsuarioCuenta cuenta = await _context.UsuarioCuenta.Where(x => x.IdUsuario == idUsuarioTransaccion).Where(x => x.Llave == "CORRIENTE").FirstOrDefaultAsync();
            mov.IdUsuario = idUsuarioTransaccion;
            mov.Monto = t.monto;
            mov.Idcuenta = cuenta.Id;
            mov.Tipo = 1;
            mov.MovContable = 0;
            mov.Usuario = "USRPULLI";
            mov.Ubicacion = "APP PULLI";
            char[] separador = { '-' };

            string[] lista = t.descripcion.Split(separador);
            long buscar = (long)Double.Parse(lista[1]);
            UsuarioTarjeta tr = _context.UsuarioTarjeta.Where(x => x.Id == buscar).FirstOrDefault();
            mov.Descripcion = lista[0] + " ****" + tr.Tarjeta.Substring(11, 4);
            mov.Documento = t.documento;
            mov.Fecha = DateTime.Now;
            mov.Sfecha = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            cuenta.Saldo = cuenta.Saldo + mov.Monto;
            try
            {
                _context.UsuarioMovimiento.Add(mov);
                _context.UsuarioCuenta.Update(cuenta);
                _context.SaveChanges();
                respuesta.resultado = true;
            }
            catch (Exception e) {
                respuesta.resultado = false;
                respuesta.mensaje = "error";
            }


            return respuesta;
        }

        [HttpPost("CompraTienda")]
        public async Task<clsRespuesta> Compra([FromBody] clsReqTransaccion t)
        {
            UsuarioMovimiento mov = new UsuarioMovimiento();
            clsRespuesta respuesta = new clsRespuesta();
            UsuarioExterno usr = await _context.UsuarioExterno.Where(x => x.Hash == t.usuario).FirstOrDefaultAsync();
            long idUsuarioTransaccion = long.Parse(usr.IdUsuario.ToString());
            UsuarioCuenta cuenta = await _context.UsuarioCuenta.Where(x => x.IdUsuario == idUsuarioTransaccion).Where(x => x.Llave == "CORRIENTE").FirstOrDefaultAsync();
            mov.IdUsuario = idUsuarioTransaccion;
            mov.Monto = t.monto;
            mov.Idcuenta = cuenta.Id;
            mov.Tipo = 0;
            mov.MovContable = 0;
            mov.Usuario = "Pulli App";
            mov.Ubicacion = _context.EmpresaSocia.Where(x => x.Id == t.strEmpresa).First().Nombre;
            mov.Descripcion = t.descripcion;
            mov.Documento = t.documento;
            mov.Fecha = DateTime.Now;
            mov.Sfecha = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            cuenta.Saldo = cuenta.Saldo - mov.Monto;

            Configuracion moduloPuntos = _context.Configuracion.Where(x => x.Llave == "Modulo Puntos").FirstOrDefault();
            Configuracion porcentaje = _context.Configuracion.Where(x => x.Llave == "Porcentaje Puntos").FirstOrDefault();
            if (moduloPuntos.Valor == "1") {
                UsuarioMovimiento movPuntos = new UsuarioMovimiento();
                UsuarioCuenta cuentaPuntos = await _context.UsuarioCuenta.Where(x => x.IdUsuario == idUsuarioTransaccion).Where(x => x.Llave == "PUNTOS").FirstOrDefaultAsync();
                movPuntos.IdUsuario = idUsuarioTransaccion;
                decimal montoPuntos = t.monto * (decimal.Parse(porcentaje.Valor) / 100);
                movPuntos.Monto = montoPuntos;
                movPuntos.Idcuenta = cuentaPuntos.Id;
                movPuntos.Tipo = 1;
                movPuntos.MovContable = 0;
                movPuntos.Usuario = "Pulli App";
                movPuntos.Descripcion = t.descripcion;
                movPuntos.Documento = t.documento;
                movPuntos.Fecha = DateTime.Now;

                cuentaPuntos.Saldo = cuentaPuntos.Saldo + movPuntos.Monto;
                _context.UsuarioMovimiento.Add(movPuntos);
                _context.UsuarioCuenta.Update(cuentaPuntos);
            }

            try
            {
                _context.UsuarioMovimiento.Add(mov);
                _context.UsuarioCuenta.Update(cuenta);
                _context.SaveChanges();
                respuesta.resultado = true;
            }
            catch (Exception e)
            {
                respuesta.resultado = false;
                respuesta.mensaje = "error";
            }


            return respuesta;
        }

        [HttpGet("obtenerMontoPromocional")]
        public async Task<clsRespuesta> obtenerMontoPromocional(string strIdPromocion, string strMonto, string strIdUsuario) {
            clsRespuesta respuesta = new clsRespuesta();
            var promocion = await _context.Promocion.Where(x => x.Id == Int32.Parse(strIdPromocion)).FirstOrDefaultAsync();
            //Validar si entrea en fechas.
            if (promocion != null)
            {
                respuesta.resultado = true;
                if (DateTime.Now < promocion.Fechafin && DateTime.Now <= promocion.FechaInicio) {
                    //valido
                    respuesta.data = promocion.MontoAplicar;
                }
            }
            else {
                respuesta.resultado = false;
            }
            return respuesta;
        }

        [HttpGet("obtenerTarjetasRegalo")]
        public async Task<clsRespuesta> obtenerTarjetasRegalo() {
            clsRespuesta respuesta = new clsRespuesta();
            List<TarjetaRegalo> tarjetas = await _context.TarjetaRegalo.ToListAsync();
            respuesta.data = tarjetas;
            respuesta.resultado = true;
            return respuesta;
        }

        [HttpGet("obtenerAutorizacion")]
        public async Task<clsRespuesta> obtenerAutorizacion(String usuario) {
            clsRespuesta respuesta = new clsRespuesta();
            UsuarioExterno usrActivo = await _context.UsuarioExterno.Where(x => x.Hash == usuario).FirstOrDefaultAsync();
            await _contextSP.actualizarAutorizacionAsync(usrActivo.IdUsuario);
            Autorizacion auth = await _context.Autorizacion.Where(x => x.IdUsuario == usrActivo.IdUsuario).FirstOrDefaultAsync();
            var datos = new { valor = auth.Valor };
            respuesta.data = datos;
            respuesta.resultado = true;
            return respuesta;

        }
        [HttpGet("actualizarAutorizacion")]
        public async Task<clsRespuesta> actualizarAutorizacion()
        {
            clsRespuesta respuesta = new clsRespuesta();

            respuesta.resultado = true;
            return respuesta;

        }

        [HttpGet("InstitucionesFin")]
        public async Task<clsRespuesta> obtenerInstituciones()
        {
            clsRespuesta respuesta = new clsRespuesta();
            try
            {
                List<Institucionesfinancieras> bancos = await _context.Institucionesfinancieras.Where(x => x.Tipo == 1).OrderByDescending(x => x.Peso).ToListAsync();
                respuesta.data = bancos;
                respuesta.resultado = true;
            }
            catch (Exception e)
            {
                respuesta.resultado = false;
                respuesta.mensaje = "No hay Instituciones";
            }
            return respuesta;
        }
        [HttpPost("crearTransferencia")]
        public async Task<clsRespuesta> crearCampana([FromBody] clsTransferencia objCampana)
        {
            clsRespuesta respuesta = new clsRespuesta();
            objCampana.encabezado.FechaCreacion = System.DateTime.Now;

            try
            {
                _context.Campana.Add(objCampana.encabezado);
                await _context.SaveChangesAsync();
                foreach (TransaccionesCampana tr in objCampana.detalle) {
                    tr.IdCampana = objCampana.encabezado.Id;
                    _context.TransaccionesCampana.Add(tr);
                }
                await _context.SaveChangesAsync();
                respuesta.resultado = true;
                respuesta.data = objCampana.encabezado.Id;
            }
            catch (Exception e)
            {
                respuesta.resultado = false;
                respuesta.mensaje = "No se pudo crear la Campana";
            }
            return respuesta;
        }
        [HttpGet("enviarGift")]
        public async Task<clsRespuesta> enviarTarjetaRegalo(string usuario,decimal monto, string destinatario, string nombre, string descripcion, int idImagen) {
            clsRespuesta respuesta = new clsRespuesta();
            //validar saldo
            UsuarioExterno ext = await _context.UsuarioExterno.Where(x => x.Hash == usuario).FirstOrDefaultAsync();
            UsuarioCuenta cuenta = await _context.UsuarioCuenta.Where(x => x.IdUsuario == ext.IdUsuario).Where(x => x.Llave == "CORRIENTE").FirstOrDefaultAsync();
            if (cuenta.Saldo >= monto)
            {
                operaciones op = new operaciones();
                UsuarioMovimiento mov = op.crearDebito((long)ext.IdUsuario,monto,cuenta.Id,"AppPulli","AppPulli","Tarjeta de regalo","4545646");
                cuenta.Saldo = cuenta.Saldo - monto;
                _context.UsuarioMovimiento.Add(mov);
                _context.UsuarioCuenta.Update(cuenta);
                _context.SaveChanges();
                respuesta.resultado = true;
                

            }
            else {
                respuesta.resultado = false;
                respuesta.mensaje="No posee el saldo para la operacion";
            }

            return respuesta;

        } 
    }
}

