using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using apiBillFold.Externo;
using apiBillFold.Models;
using Google.Type;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace apiBillFold.Controllers
{
    [Route("api/[controller]")]
    public class AdministradorController : Controller
    {
        private readonly BilleteraCMContext _context;
        private readonly BilleteraCMContextProcedures _contextSP;
        private fireBaseController fireb = new fireBaseController();
        public IConfiguration _configuration;

        public AdministradorController(BilleteraCMContext context, IConfiguration configuration)
        {
            _context = context;
            _contextSP = new BilleteraCMContextProcedures(_context);
            _configuration = configuration;
        }
        #region Bitacora
        [HttpPost("bitacora")]
        public async Task<ActionResult<clsRespuesta>> agregarBitacora([FromBody] Bitacora bit)
        {
            clsRespuesta resp = new clsRespuesta();
            try {
                _context.Bitacora.Add(bit);
                await _context.SaveChangesAsync();
                resp.resultado = true;

            }
            catch (Exception e) {
                resp.resultado = false;
            }

            return resp;

        }
        #endregion

        #region Ingreso
        [HttpGet("spIngreso")]
        public async Task<ActionResult<ManejoIngresoResult>> ManejarLoginSP(string usuario, string password)
        {
            var usuario1 = await _contextSP.ManejoIngresoAsync(usuario, password);

            if (usuario1.Count == 0)
            {
                return NotFound();
            }
            //  var bitacora = await _contextSP.agregarBitacoraIngresoAsync(usuario1[0].id);
            return usuario1[0];
        }

        [HttpGet("empresaAsociada/{id}")]
        public async Task<ActionResult<EmpresaSocia>> GetempresaSocium(int id)
        {
            var empresaSocium = await _context.EmpresaSocia.FindAsync(id);

            if (empresaSocium == null)
            {
                return NotFound();
            }

            return empresaSocium;
        }

        [HttpGet("spGetMenuAdministrador")]
        public async Task<List<obtenerMenuAdministradorResult>> generarMenuAdministrador()
        {
            OutputParameter<int?> resultado = new OutputParameter<int?>();
            var respuesta = await _contextSP.obtenerMenuAdministradorAsync();
            if (respuesta.Count == 0)
            {
                return null;
            }

            return respuesta;
        }

        [HttpGet("pais")]
        public async Task<ActionResult<IEnumerable<Pais>>> Getpais()
        {
            return _context.Pais.Where(x => x.Activo == 1).ToList();
        }

        [HttpGet("spGetMenu")]
        public async Task<List<obtenerMenuResult>> generarMenu(long usuario)
        {
            OutputParameter<int?> resultado = new OutputParameter<int?>();
            var respuesta = await _contextSP.obtenerMenuAsync(usuario);

            if (respuesta.Count == 0)
            {
                return null;
            }
            return respuesta;
        }

        [HttpGet("enviarRecuperacionAdmin")]
        public async Task<int> getAdministracionRecuperacion(String data)
        {
            try
            {
                seguridad _seg = new seguridad();
                String sniffer = HttpUtility.UrlDecode(data);
                String desencript = _seg.Decrypt(sniffer, seguridad.appPwdUnique, int.Parse("256"));
                String[] separar = desencript.Split("-");
                if (separar.Length == 2)
                {
                    String correo = separar[0].Replace("email:", "");
                    var administrador = await _context.Administrador.Where(x => x.Alias.ToUpper().Equals(correo.ToUpper())).FirstOrDefaultAsync();// .Where(x => x.idEmpresa == id).ToList();
                    if (administrador.Alias != null)
                    {
                        var formatoCorreo = await _context.TextosMail.Where(x => x.Descripcion == "RecuperacionAdmin").FirstOrDefaultAsync();
                        String Cuerpo = String.Format(formatoCorreo.Texto, data, administrador.PrimerNombre + " " + administrador.PrimerApellido);
                        seguridad.enviarCorreo(administrador.Alias, Cuerpo, "Recuperacion Contraseña");
                        return 1;
                    }
                    return -3;

                }
                else
                {
                    return -2;
                }
            }
            catch (Exception e)
            {

                return -1;
            }



        }

        [HttpGet("actualizarPassword")]
        public async Task<int> updatePassword(String nuevo, String data)
        {
            try
            {
                seguridad _seg = new seguridad();
                String sniffer = HttpUtility.UrlDecode(data);
                String snifferNuevo = HttpUtility.UrlDecode(nuevo);
                String desencript = _seg.Decrypt(sniffer, seguridad.appPwdUnique, int.Parse("256"));
                String desencriptNuevo = _seg.Decrypt(snifferNuevo, seguridad.appPwdUnique, int.Parse("256"));
                String[] separar = desencript.Split("-");
                if (separar.Length == 2)
                {
                    String correo = separar[0].Replace("email:", "");
                    var administrador = await _context.Administrador.Where(x => x.Alias.ToUpper().Equals(correo.ToUpper())).FirstOrDefaultAsync();// .Where(x => x.idEmpresa == id).ToList();
                    if (administrador.Alias != null)
                    {
                        await _contextSP.updateContrasenaAsync(desencriptNuevo, administrador.Id);
                        return 1;
                    }
                    return -3;

                }
                else
                {
                    return -2;
                }
            }
            catch (Exception e)
            {

                return -1;
            }
        }



        #endregion

        #region Configuraciones

        [HttpGet("configuracion/porPais")]
        public async Task<ActionResult<IEnumerable<Configuracion>>> GetconfiguracionsPorPais(int id, int tipo)
        {
            return await _context.Configuracion.Where(x => x.Idpais == id).Where(y => y.Tipo == tipo).ToListAsync();
        }

        [HttpGet("configuracion/porPaisPorllave")]
        public async Task<ActionResult<IEnumerable<Configuracion>>> getConfiguracionPorPaisLlave(int idPais, string llave)
        {
            var a = await _context.Configuracion.Where(x => x.Idpais == idPais).ToListAsync();
            var b = a.Where(x => x.Llave == llave).ToList();
            return b;
        }

        [HttpGet("configuracion/ConfiguracionesMail")]
        public async Task<ActionResult<IEnumerable<TextosMail>>> configuracionesMail(int idpais)
        {
            var a = await _context.TextosMail.Where(x => x.IdPais == idpais).ToListAsync();
            return a.OrderByDescending(x => x.Descripcion).ToList();

        }

        [HttpGet("configuracion/ActualizarConfiguracionMail")]
        public async Task<ActionResult<int>> ModifarConfiguracionMail(long id, string nuevoMensaje)
        {
            var textoMail = await _context.TextosMail.FindAsync(id);
            if (textoMail == null)
            {
                return -1;
            }
            else
            {
                textoMail.Texto = HttpUtility.HtmlDecode(nuevoMensaje);
                _context.Entry(textoMail).State = EntityState.Modified;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return -1;
                }
                return 0;
            }

        }

        #endregion

        #region Grupos
        // GET: api/grupoes
        [HttpGet("grupos")]
        public async Task<ActionResult<IEnumerable<Grupo>>> Getgrupos()
        {
            var a = await _context.Grupo.ToListAsync();
            var b = a.Where(x => x.Estado == 1).ToList();
            return b;
        }
        [HttpGet("grupos/porPais/{id}")]
        public async Task<ActionResult<IEnumerable<Grupo>>> Getgrupos(int id)
        {
            var a = await _context.Grupo.ToListAsync();
            var b = a.Where(x => x.Estado == 1).ToList();
            var c = b.Where(x => x.Idpais == id).ToList();
            return c;
        }
        // GET: api/grupoes/5
        [HttpGet("grupos/{id}")]
        public async Task<ActionResult<Grupo>> Getgrupo(long id)
        {
            var grupo = await _context.Grupo.FindAsync(id);

            if (grupo == null)
            {
                return NotFound();
            }

            return grupo;
        }

        // PUT: api/grupoes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("grupos/{id}")]
        public async Task<IActionResult> Putgrupo(long id, Grupo grupo)
        {
            if (id != grupo.Id)
            {
                return BadRequest();
            }

            _context.Entry(grupo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!grupoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/grupoes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("grupos")]
        public async Task<ActionResult<Grupo>> Postgrupo([FromBody] Grupo grupo)
        {
            _context.Grupo.Add(grupo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGrupo", new { id = grupo.Id }, grupo);
        }
        [HttpGet("grupos/DetalleGrupo/porPais/{id}")]
        public async Task<List<Detallegrupo>> GetdetallegrupoPorPais(int id)
        {
            var detallegrupo = await _contextSP.getAsociacionDetallePaisAsync(id);
            List<Detallegrupo> d = new List<Detallegrupo>();

            if (detallegrupo == null)
            {
                return d;
            }
            foreach (var moldear in detallegrupo)
            {
                Detallegrupo a = new Detallegrupo();
                a.Id = moldear.id;
                a.IdGrupo = moldear.idGrupo;
                a.Idempresa = moldear.idempresa;
                a.Fecha = moldear.fecha;
                a.Idpais = moldear.idpais;
                d.Add(a);
            }
            return d;
        }

        [HttpPost("DetalleGrupo")]
        public async Task<clsRespuesta> agregarDetalleGrupo([FromBody] Detallegrupo dg ) {
            clsRespuesta resp = new clsRespuesta();
            try
            {
                resp.resultado = true;
                _context.Detallegrupo.Add(dg);
                await _context.SaveChangesAsync();
            }
            catch (Exception e) {
                resp.resultado = false;
            }
            return resp;
        }
        // DELETE: api/grupoes/5
        [HttpDelete("Grupos/detallegrupoes/{id}")]
        public async Task<IActionResult> Deletegrupo(long id)
        {
            var grupo = await _context.Detallegrupo.FindAsync(id);
            if (grupo == null)
            {
                return NotFound();
            }

            _context.Detallegrupo.Remove(grupo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool grupoExists(long id)
        {
            return _context.Grupo.Any(e => e.Id == id);
        }


        #endregion

        #region Empresa

        [HttpGet("EmpresasSocias/porPais/{id}")]
        public async Task<ActionResult<IEnumerable<EmpresaSocia>>> GetEmpresas(int id)
        {
            return await _context.EmpresaSocia.ToListAsync();
        }

        [HttpPost("empresaSociums")]
        public async Task<ActionResult<EmpresaSocia>> PostempresaSocium([FromBody] EmpresaSocia empresaSocium)
        {
            _context.EmpresaSocia.Add(empresaSocium);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetempresaSocium", new { id = empresaSocium.Id }, empresaSocium);
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

        [HttpGet("tipoCuenta")]
        public async Task<clsRespuesta> obtenerTipoCuenta()
        {
            clsRespuesta respuesta = new clsRespuesta();
            try
            {
                List<TiposCuenta> tiposCuenta = await _context.TiposCuenta.ToListAsync();
                respuesta.data = tiposCuenta;
                respuesta.resultado = true;
            }
            catch (Exception e)
            {
                respuesta.resultado = false;
                respuesta.mensaje = "No hay Tipos Cuenta";
            }
            return respuesta;
        }

        #endregion

        #region Administrador

        [HttpGet("Empresa/{id}")]
        public async Task<ActionResult<IEnumerable<Administrador>>> GetadministradorsByEmpresa(int id)
        {
            var administradores = _context.Administrador.Where(x => x.IdEmpresa == id).ToList();
            if (administradores == null)
            {
                return NotFound();
            }
            return administradores;
        }

        [HttpGet("obtenerRolesTipoEmpresa")]
        public async Task<ActionResult<IEnumerable<Roles>>> obtenerRolesPorTipoEmpresa(long idEmpresa) {
            EmpresaSocia socio = _context.EmpresaSocia.Where(x => x.Id == idEmpresa).FirstOrDefault();
            var roles = _context.Roles.Where(x => x.TipoEmpresa == socio.TipoEmpresa).ToList();
            if (roles == null) {
                return new List<Roles>();
            }
            return roles;

        }

        [HttpGet("obtenerRoles")]
        public async Task<ActionResult<IEnumerable<Roles>>> obtenerRoles()
        {
            var roles = _context.Roles.ToList();
            if (roles == null)
            {
                return new List<Roles>();
            }
            return roles;

        }

        [HttpGet("spAlmacenaAdministrador")]
        public async Task<int> agregarAdministrador(string primerNombre, string segundoNombre, string primerApellido,
        string segundoApellido, long DPI, string strUsuario, int idEmpresa, long idUsuarioActivo, int tipo)
        {
            segundoNombre = limpiar(segundoNombre);
            segundoApellido = limpiar(segundoApellido);
            Random rand = new Random();
            String randomPass = rand.Next().ToString();
            var respuesta = await _contextSP.agregarAdministradorAsync(primerNombre, segundoNombre, primerApellido, segundoApellido, DPI, strUsuario, idEmpresa, randomPass, tipo);

            if (respuesta == 0)
            {
                return -1;
            }
            long a = 3;
            var correo = await _context.TextosMail.FindAsync(a);
            Bitacora mibitacora = new Bitacora();
            mibitacora.Fecha = System.DateTime.Now;
            mibitacora.Funcion = "Agregar Administrador";
            mibitacora.Idempresa = idEmpresa;
            mibitacora.Descripcion = "Agregando el Administrador " + primerNombre + " " + segundoNombre + " " + primerApellido + " " + segundoApellido;
            var UsuarioActivo = await _context.Administrador.FindAsync(idUsuarioActivo);
            mibitacora.Usuario = UsuarioActivo.Alias;
            _context.Bitacora.AddAsync(mibitacora);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            //            String body = String.Format(correo.1, strUsuario, randomPass);
            //          enviarCorreo(strUsuario, body, "Alta Usuario");
            return 0;
        }

        [HttpGet("reinicioAdmin")]
        public async Task<int> reinicioAdministrador(int id, long idUsuarioActivo)
        {
            var administradores = await _context.Administrador.ToListAsync(); // .Where(x => x.idEmpresa == id).ToList();
            if (administradores == null)
            {
                return 0;
            }
            var lista = administradores.Where(x => x.Id == id).ToList(); ;
            var admin = lista.First();
            long a = 3;
            var correo = await _context.TextosMail.FindAsync(a);
            Random rand = new Random();
            String randomPass = rand.Next().ToString();
            if (correo != null)
            {
                String cuerpoCorreo = String.Format(correo.Texto, admin.Alias, randomPass);
                var b = await _contextSP.updateContrasenaAsync(randomPass, admin.Id);
                seguridad.enviarCorreo(admin.Alias, cuerpoCorreo, "Reinicio Contraseña");
                var admin2 = await _context.Administrador.FindAsync(idUsuarioActivo);
                Bitacora bita = new Bitacora();
                bita.Funcion = "Reinicio Contraseña Administrador";
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                bita.Usuario = admin2.Alias;
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
                bita.Fecha = System.DateTime.Now;
                bita.Idempresa = admin2.IdEmpresa;
                bita.Descripcion = "Se envio reinicio a: " + admin.Alias;
#pragma warning disable CS4014 // Como esta llamada no es 'awaited', la ejecución del método actual continuará antes de que se complete la llamada. Puede aplicar el operador 'await' al resultado de la llamada.
                await _context.Bitacora.AddAsync(bita);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return 0;
                }
            }



            return 1;
        }




        // GET: api/administradors/5
        [HttpGet("Admin/{id}")]
        public async Task<ActionResult<Administrador>> Getadministrador(long id)
        {
            var administrador = await _context.Administrador.FindAsync(id);

            if (administrador == null)
            {
                return NotFound();
            }

            return administrador;
        }
        [HttpGet("bloqueo")]
        public async Task<ActionResult<int>> bloqueo(long id, long idUsuarioActivo)
        {
            var administrador = await _context.Administrador.FindAsync(id); ;
            if (administrador == null)
            {
                return NotFound();
            }
            administrador.Idtipo = 0;
            var admin = await _context.Administrador.FindAsync(idUsuarioActivo);
            Bitacora b = new Bitacora();
            b.Funcion = "Bloquear Administrador";
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
            b.Usuario = admin.Alias;
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
            b.Fecha = System.DateTime.Now;
            b.Idempresa = admin.IdEmpresa;
            b.Descripcion = "Bloqueo de administrador " + administrador.Alias;
#pragma warning disable CS4014 // Como esta llamada no es 'awaited', la ejecución del método actual continuará antes de que se complete la llamada. Puede aplicar el operador 'await' al resultado de la llamada.
            _context.Bitacora.AddAsync(b);
#pragma warning restore CS4014 // Como esta llamada no es 'awaited', la ejecución del método actual continuará antes de que se complete la llamada. Puede aplicar el operador 'await' al resultado de la llamada.
            _context.Entry(administrador).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return -1;
            }
            return 0;
        }

        [HttpGet("desbloqueo")]
        public async Task<ActionResult<int>> desbloqueo(long id, long idUsuarioActivo)
        {
            var administrador = await _context.Administrador.FindAsync(id); ;
            if (administrador == null)
            {
                return NotFound();
            }
            administrador.Idtipo = 1;
            var admin = await _context.Administrador.FindAsync(idUsuarioActivo);
            Bitacora b = new Bitacora();
            b.Funcion = "Desbloquear Administrador";
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
            b.Usuario = admin.Alias;
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
            b.Fecha = System.DateTime.Now;
            b.Idempresa = admin.IdEmpresa;
            b.Descripcion = "Desbloqueo de administrador " + administrador.Alias;
#pragma warning disable CS4014 // Como esta llamada no es 'awaited', la ejecución del método actual continuará antes de que se complete la llamada. Puede aplicar el operador 'await' al resultado de la llamada.
            _context.Bitacora.AddAsync(b);
#pragma warning restore CS4014 // Como esta llamada no es 'awaited', la ejecución del método actual continuará antes de que se complete la llamada. Puede aplicar el operador 'await' al resultado de la llamada.
            _context.Entry(administrador).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return -1;
            }
            return 0;
        }

        // PUT: api/administradors/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpGet("actualizarPerfil")]
        public async Task<int> Putadministrador(long id, String PrimerNombre, String SegundoNombre, String PrimerApellido, String SegundoApellido, String Alias, long usuarioModifica)
        {
            var admin = await _context.Administrador.FindAsync(id);

            try
            {
                if (admin != null)
                {

                    admin.PrimerNombre = PrimerNombre;
                    admin.SegundoNombre = SegundoNombre;
                    admin.PrimerApellido = PrimerApellido;
                    admin.SegundoApellido = SegundoApellido;
                    admin.Alias = Alias;
                    Bitacora b = new Bitacora();
                    b.Funcion = "Actualizar Administrador";
                    b.Usuario = admin.Alias;
                    b.Descripcion = "Usuario Actualizado" + admin.PrimerNombre + " " + admin.PrimerApellido;
                    b.Fecha = System.DateTime.Now;
                    b.Idempresa = admin.IdEmpresa;
                    _context.Entry(admin).State = EntityState.Modified;
                    await _context.Bitacora.AddAsync(b);
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {

                    }


                }

            }
            catch (Exception)
            {

                return 0;

            }


            return 1;
        }


        #endregion

        #region EmpleadosUsuarios

        [HttpGet("crearCliente")]
        public async Task<ActionResult<int>> crearCliente(string pnombre, string snombre, string otronombre,
        string primerApellido, string segundoApellido, string apellidoCasada, string nit, string alias, long idEmpresa, int telefono)
        {
            OutputParameter<long?> opa = new OutputParameter<long?>();
            snombre = limpiar(snombre);
            otronombre = limpiar(otronombre);
            segundoApellido = limpiar(segundoApellido);
            apellidoCasada = limpiar(apellidoCasada);
            nit = limpiar(nit);
            var respuesta = await _contextSP.crearClienteAsync(pnombre, snombre, otronombre, primerApellido, segundoApellido, apellidoCasada, nit, alias, idEmpresa, telefono, opa);
            if (respuesta == null)
            {
                return 0;
            }
            return 1;
        }

        [HttpPost("usuarios")]
        public async Task<ActionResult<int>> crearEmpleado([FromBody] Usuario datosUsuario)
        {
            OutputParameter<long?> opa = new OutputParameter<long?>();
            datosUsuario.SegundoNombre = limpiar(datosUsuario.SegundoNombre);
            datosUsuario.OtrosNombres = limpiar(datosUsuario.OtrosNombres);
            datosUsuario.SegundoApellido = limpiar(datosUsuario.SegundoApellido);
            datosUsuario.ApellidoCasada = limpiar(datosUsuario.ApellidoCasada);
            datosUsuario.PrimerApellido = limpiar(datosUsuario.PrimerApellido);

            datosUsuario.Nit = limpiar(datosUsuario.Nit);
            var respuesta = await _contextSP.crearClienteAsync(datosUsuario.PrimerNombre, datosUsuario.SegundoNombre, datosUsuario.OtrosNombres, datosUsuario.PrimerApellido, datosUsuario.SegundoApellido, datosUsuario.ApellidoCasada, datosUsuario.Nit, datosUsuario.Alias, datosUsuario.IdEmpresa, 0, opa);
            if (respuesta == null)
            {
                return 0;
            }
            return 1;
        }

        private string limpiar(String texto)
        {
            if (texto.Equals("empty"))
            {
                return "";
            }
            return texto;
        }

        [HttpGet("spBuscarCliente")]
        public async Task<ActionResult<List<buscarClienteResult>>> ClientePorTelefono(string valor, int opcion)
        {
            string telefono = "";
            string mail = "";
            string nit = "";

            switch (opcion)
            {
                case 1:
                    telefono = valor;
                    break;
                case 2:
                    mail = valor;
                    break;
                case 3:
                    nit = valor;
                    break;

            }
            var usuario1 = await _contextSP.buscarClienteAsync(telefono, mail, nit, opcion);

            if (usuario1 == null)
            {
                return NotFound();
            }

            return usuario1;
        }

        [HttpGet("spAlmacenarUsuario")]
        public async Task<string> agregarUsuario(string primerNombre, string segundoNombre, string otroNombre, string primerApellido,
        string segundoApellido, string casadaApellido, string codigoEmpleado, string NIT, decimal salario, decimal disponible,
        string DPI, int idEmpresa, string cumple, string mails, string telefonos, string direcciones, string familiares, long idUsuarioActivo)
        {
            segundoNombre = limpiar(segundoNombre);
            otroNombre = limpiar(otroNombre);
            segundoApellido = limpiar(segundoApellido);
            casadaApellido = limpiar(casadaApellido);
            NIT = limpiar(NIT);
            //validaciones
            string strUsuario = mails.Split('|')[0];
            OutputParameter<string> p = new OutputParameter<string>();
            // var resultado = await _contextSP.validarClienteAsync(telefonos.Split('|')[0].Split('-')[0], strUsuario, DPI, idEmpresa, p);
            if (true)
            {
                OutputParameter<long?> idEmpleado = new OutputParameter<long?>();
                String hash = "";
                var respuesta = await _contextSP.agregarUsuarioAsync(primerNombre, segundoNombre, otroNombre, primerApellido, segundoApellido, casadaApellido, cumple, codigoEmpleado,
                NIT, salario, disponible, DPI, strUsuario, 1, idEmpresa, hash, idEmpleado);

                if (!mails.Contains("NOEXISTE"))
                {
                    String[] sepMail = mails.Split('|');
                    for (int i = 0; i < sepMail.Length; i++)
                    {
                        String[] correo = sepMail[i].Split('@');
                        UsuarioMail newMail = new UsuarioMail();
                        newMail.IdUsuario = (int)idEmpleado.Value;
                        newMail.Encabezado = correo[0];
                        newMail.Servidor = correo[1];
                        newMail.Completo = sepMail[i];
                        var respuestab = await _context.UsuarioMail.AddAsync(newMail);
                    }
                }
                if (!telefonos.Contains("NOEXISTE"))
                {
                    String[] arrayTelefonos = telefonos.Split('|');
                    for (int i = 0; i < arrayTelefonos.Length; i++)
                    {
                        String[] tels = arrayTelefonos[i].Split('-');
                        UsuarioTelefono tel = new UsuarioTelefono();
#pragma warning disable CS8629 // Un tipo que acepta valores NULL puede ser nulo.
                        tel.IdEmpleado = (int)idEmpleado.Value;
#pragma warning restore CS8629 // Un tipo que acepta valores NULL puede ser nulo.
                        tel.Tipo = 1;
                        tel.Numero = tels[0];
                        tel.Alias = tels[1];
                        var respuestab = await _context.UsuarioTelefono.AddAsync(tel);
                    }
                }



                Bitacora miBitacora = new Bitacora();
                miBitacora.Funcion = "Agregar Usuario";
                miBitacora.Idempresa = idEmpresa;
                miBitacora.Fecha = System.DateTime.Now;
                var UsuarioActivo = await _context.Administrador.FindAsync(idUsuarioActivo);
                miBitacora.Usuario = UsuarioActivo.Alias;
                miBitacora.Descripcion = "Creacion de usuario " + primerNombre + " " + primerApellido;
                _context.Bitacora.AddAsync(miBitacora);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return "No se pudo almacenar el cliente";

                }
                notificarEmpleado(long.Parse(idEmpleado.Value.ToString()));




                return "";
            }
            else
            {
                return p.Value;

            }

        }

        [HttpGet("UsuarioXEmpresa/{id}")]
        public async Task<ActionResult<IEnumerable<Usuario>>> getusuariobyEmpresa(long id)
        {
            var usuario = await _context.Usuario.ToListAsync();

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario.Where(x => x.IdEmpresa == id).ToList();
        }


        #endregion

        #region Contabilidad
        [HttpGet("ObtenerCuentas")]
        public async Task<clsRespuesta> obtenerCuentasCon(double idConsultor)
        {
            clsRespuesta respuesta = new clsRespuesta();

            if (idConsultor == 1)
            {
                var cuentas = await _context.CuentaEmpresa.ToListAsync();
                respuesta.data = cuentas.ToList();
                respuesta.resultado = true;
            }
            else
            {
                Administrador user = await _context.Administrador.Where(x => x.Id == idConsultor).FirstOrDefaultAsync();
                var cuentas = await _context.CuentaEmpresa.Where(x => x.IdEmpresa == user.IdEmpresa).ToListAsync();
                respuesta.data = cuentas.ToList();
                respuesta.resultado = true;

            }
            return respuesta;
        }

        [HttpGet("ObtenerCuentasMovimientos")]
        public async Task<clsRespuesta> obtenerCuentasConMovimientos(long idCuenta)
        {
            clsRespuesta respuesta = new clsRespuesta();
            var cuentas = await _context.CuentaEmpresaMovimiento.Where(x => x.Idcuenta == idCuenta).ToListAsync();
            respuesta.data = cuentas.ToList();
            respuesta.resultado = true;

            return respuesta;
        }


        #endregion

        #region Ventas
        [HttpPost("GenerarAutorizacion")]
        public async Task<int> GenerarTokenAutorizacion(string mail, decimal monto, int comercio)
        {
            String strComercio = "";
            String token = "";
            String deviceToken = "";

            //buscar Token Asociado
            var result = await fireb.obtenerTokenUsuario(mail);
            if (result.Value.Equals("No Existe"))
            {
                return -1;
            }
            else
            {
                deviceToken = result.Value;
                //verificarEmpresa
                EmpresaSocia empresa = _context.EmpresaSocia.Where(x => x.Id == comercio).Where(y => y.Estado == 1).FirstOrDefault();
                if (empresa != null)
                {
                    strComercio = empresa.Nombre;
                    //Crear Parametro autorizacion Temporal
                    OutputParameter<int?> pResultado = new OutputParameter<int?>();
                    OutputParameter<decimal?> pToken = new OutputParameter<decimal?>();
                    var resultado = await _contextSP.generarTokenAsync(mail, monto, comercio, pResultado, pToken);
                    if (pResultado.Value == 1)
                    {
                        //Enviar puush notificacion
                        token = pToken.Value.ToString();
                        await fireb.enviarTokenAutorizacion("Numero Autorizacion", "Autorizacion: " + token + " para completar tu compra por Q" + monto.ToString() + " en " + strComercio, deviceToken, token);
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }
            }
            return 0;
        }

        [HttpPost("spGenerarTransaccion")]
        public async Task<clsRespuesta> generarCompra(long id, decimal monto, decimal token, string detalle, long idVendedor, int TipoComercio)
        {
            operaciones op = new operaciones();
            clsRespuesta objRespuesta = new clsRespuesta();
            try
            {
                if (TipoComercio==2) {
                    OutputParameter<short?> resultado = new OutputParameter<short?>();
                    var respuestaDB = await _contextSP.validarTokenAsync(id, token, resultado);
                    if (resultado.Value == 1)
                    {
                        objRespuesta.resultado = true;
                        List<dataOrdenes> ordenes = JsonConvert.DeserializeObject<List<dataOrdenes>>(detalle);
                        Usuario user = _context.Usuario.Where(x => x.Id == id).FirstOrDefault();
                        Administrador admUser = await _context.Administrador.Where(x => x.Id == idVendedor).FirstOrDefaultAsync();
                        EmpresaSocia empSocia = await _context.EmpresaSocia.Where(x => x.Id == admUser.IdEmpresa).FirstOrDefaultAsync();
                        List<getNumeroOrdenResult> autNumber = await _contextSP.getNumeroOrdenAsync();
                        getNumeroOrdenResult autreference = autNumber.ElementAt<getNumeroOrdenResult>(0);
                        double autorizacion = double.Parse(autreference.noOrden.ToString());
                        string strDetalle = "";
                        decimal total = 0;
                        foreach (dataOrdenes d in ordenes)
                        {
                            String temporal = "<tr><td>" + d.cantidad + "</td><td>" + d.nombre + "</td><td>Q" + d.precio + "</td><td>Q" + (decimal.Parse(d.precio) * decimal.Parse(d.cantidad)).ToString() + "</td></tr>";
                            strDetalle += temporal;
                            total = total += decimal.Parse((decimal.Parse(d.precio) * decimal.Parse(d.cantidad)).ToString());
                        }
                        strDetalle += "<tr><td></td><td></td><td>Total</td><td>Q" + total.ToString("#.##") + "</td><tr>";
                        var textoMail = await _context.TextosMail.Where(x => x.Descripcion == "Venta").FirstOrDefaultAsync();
                        String mensajeFinal = "Has realizado la compra por medio de Pulli por un monto de Q" + monto;
                        String CuerpoCorreo = String.Format(textoMail.Texto, " " + user.PrimerNombre + " " + user.PrimerApellido, " " + user.Alias + " ", "", " " + autorizacion.ToString(), " " + System.DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), strDetalle, "", mensajeFinal);
                        seguridad.enviarCorreo(user.Alias, CuerpoCorreo, "Orden #" + autorizacion.ToString());
                        var refUUser = await fireb.obtenerUsrReference(user.Alias);
                        // fireb.crearOrden(Double.Parse(monto.ToString()), autorizacion, "Comprado", refUUser.Value, idVendedor.ToString(), ordenes, empSocia.UidTienda);
                        //fireb.agregarTransaccion("debito", monto.ToString(), "Compra en " + empSocia.Nombre, empSocia.Direccion, "debito", autorizacion.ToString(), refUUser.Value.ToString());

                    }
                    else
                    {
                        objRespuesta.mensaje = "No se pudo validar el numero de autorizacion correctamente";
                        objRespuesta.resultado = false;
                    }
                }
                else if(TipoComercio==3)
                {
                    Autorizacion auth = _context.Autorizacion.Where(x => x.Valor == token).First();
                    if (auth != null)
                    {
                        UsuarioExterno ext =  _context.UsuarioExterno.Where(x => x.IdUsuario == auth.IdUsuario).FirstOrDefault();
                        UsuarioCuenta cuenta = _context.UsuarioCuenta.Where(x => x.IdUsuario == auth.IdUsuario).Where(y => y.Llave == "CORRIENTE").First();
                        Administrador admUser =  _context.Administrador.Where(x => x.Id == idVendedor).FirstOrDefault();
                        EmpresaSocia empSocia =  _context.EmpresaSocia.Where(x => x.Id == admUser.IdEmpresa).FirstOrDefault();
                        List<getNumeroOrdenResult> autNumber = await _contextSP.getNumeroOrdenAsync();
                        getNumeroOrdenResult autreference = autNumber.ElementAt<getNumeroOrdenResult>(0);
                        UsuarioMovimiento userCredito=op.crearCredito(auth.IdUsuario, monto, cuenta.Id, admUser.Id.ToString(), empSocia.Nombre, "Recarga", autreference.noOrden.ToString());
                        double porcentaje =0.10;
                        double nuevoMonto = (double)monto*  porcentaje;
                        UsuarioMovimiento userCreditoPremio = op.crearCredito(auth.IdUsuario,decimal.Parse(nuevoMonto.ToString()), cuenta.Id, admUser.Id.ToString(), empSocia.Nombre, "Saldo Regalo Pulli", autreference.noOrden.ToString());
                        _context.UsuarioMovimiento.Add(userCredito);
                        _context.UsuarioMovimiento.Add(userCreditoPremio);
                        cuenta.Saldo = cuenta.Saldo + monto+ decimal.Parse(nuevoMonto.ToString());
                        _context.UsuarioCuenta.Update(cuenta);
                        _context.SaveChanges();
                        objRespuesta.resultado = true;
                        objRespuesta.data = autreference.ToString();
                        var result = await fireb.obtenerTokenUsuario(ext.Correo);
                        if (result.Value.Equals("No Existe"))
                        {
                        }

                        else
                        {
                            String deviceToken = result.Value;
                            await fireb.enviarTokenAutorizacion("Acreditacion Realizada", "Has realizado una recarga de : Q" + monto.ToString() +" y Saldo Promocinal de Q"+nuevoMonto.ToString(), deviceToken, monto.ToString());
                        }

                           
                        /*List<dataOrdenes> ordenes = JsonConvert.DeserializeObject<List<dataOrdenes>>(detalle);*/
                    }
                    else {
                        objRespuesta.mensaje = "No se pudo validar el numero de autorizacion correctamente";
                        objRespuesta.resultado = false;
                    }

                }


                

            }
            catch (Exception e)
            {
                objRespuesta.mensaje = "No se pudo completar la transaccion intente mas tarde";
                objRespuesta.resultado = false;
            }
            return objRespuesta;
        }

        [HttpPost("AutorizarCompraGenerica")]
        public async Task<clsRespuesta> generarCompraGenerica(long autorizacion, decimal monto,  string detalle, long idVendedor)
        {
            operaciones op = new operaciones();
            clsRespuesta objRespuesta = new clsRespuesta();
            try
            {
                    Autorizacion auth = _context.Autorizacion.Where(x => x.Valor == autorizacion).First();
                    if (auth != null)
                    {
                        UsuarioExterno ext = _context.UsuarioExterno.Where(x => x.IdUsuario == auth.IdUsuario).FirstOrDefault();
                        UsuarioCuenta cuenta = _context.UsuarioCuenta.Where(x => x.IdUsuario == auth.IdUsuario).Where(y => y.Llave == "CORRIENTE").First();
                        UsuarioCuenta cuentaPuntos = _context.UsuarioCuenta.Where(x => x.IdUsuario == auth.IdUsuario).Where(y => y.Llave == "PUNTOS").First();
                        Administrador admUser = _context.Administrador.Where(x => x.Id == idVendedor).FirstOrDefault();
                        EmpresaSocia empSocia = _context.EmpresaSocia.Where(x => x.Id == admUser.IdEmpresa).FirstOrDefault();
                        List<getNumeroOrdenResult> autNumber = await _contextSP.getNumeroOrdenAsync();
                        getNumeroOrdenResult autreference = autNumber.ElementAt<getNumeroOrdenResult>(0);
                        UsuarioMovimiento userCredito = op.crearDebito(auth.IdUsuario, monto, cuenta.Id, admUser.Id.ToString(), empSocia.Nombre, "Compra", autreference.noOrden.ToString());
                        Configuracion confPorcentaje = await _context.Configuracion.Where(x => x.Llave == "Porcentaje Puntos").FirstOrDefaultAsync();
                        double porcentaje = Int32.Parse(confPorcentaje.Valor)/100;
                        double nuevoMonto = (double)monto * porcentaje;
                        UsuarioMovimiento userCreditoPremio = op.crearCredito(auth.IdUsuario, decimal.Parse(nuevoMonto.ToString()), cuentaPuntos.Id, admUser.Id.ToString(), empSocia.Nombre, "Puntos Regalo Pulli", autreference.noOrden.ToString());
                        _context.UsuarioMovimiento.Add(userCredito);
                        _context.UsuarioMovimiento.Add(userCreditoPremio);
                    cuenta.Saldo = cuenta.Saldo - monto;
                    cuentaPuntos.Saldo= cuentaPuntos.Saldo+ decimal.Parse(nuevoMonto.ToString());
                    _context.UsuarioCuenta.Update(cuenta);
                    _context.UsuarioCuenta.Update(cuentaPuntos);
                    _context.SaveChanges();
                        objRespuesta.resultado = true;
                        objRespuesta.data = autreference.ToString();
                        var result = await fireb.obtenerTokenUsuario(ext.Correo);
                        if (result.Value.Equals("No Existe"))
                        {
                        }

                        else
                        {
                            String deviceToken = result.Value;
                            await fireb.enviarTokenAutorizacion("Compra Realizada", "Has realizado una compra por: Q" + monto.ToString() + " en: " + empSocia.Nombre + " Recibes:"+nuevoMonto.ToString()+"  puntos", deviceToken, monto.ToString());
                        TextosMail t = await _context.TextosMail.Where(x => x.Descripcion == "FACTURA").FirstOrDefaultAsync();
                        String cuerpoCorreo = String.Format(t.Texto, monto.ToString(), System.DateTime.Now.ToString());
                        seguridad.enviarCorreo(ext.Correo, cuerpoCorreo, "Notificación Factura Electrónica");
                        }


                        /*List<dataOrdenes> ordenes = JsonConvert.DeserializeObject<List<dataOrdenes>>(detalle);*/
                    }
                    else
                    {
                        objRespuesta.mensaje = "No se pudo validar el numero de autorizacion correctamente";
                        objRespuesta.resultado = false;
                    }





            }
            catch (Exception e)
            {
                objRespuesta.mensaje = "No se pudo completar la transaccion intente mas tarde";
                objRespuesta.resultado = false;
            }
            return objRespuesta;
        }


        #endregion

        [HttpGet("notificarEmpleado")]
        public async Task<ActionResult<int>> notificarEmpleado(long empleado)
        {

            string valorCorreo = "BIENVENIDA CLIENTE";
            var MailsEmpleado = await _context.UsuarioMail.Where(x => x.IdUsuario == empleado).ToListAsync();
            var dataEmpleado = await _context.Usuario.Where(x => x.Id == empleado).ToListAsync();
            Usuario usr = dataEmpleado.First();
            TextosMail Cuerpocorreo = await _context.TextosMail.Where(x => x.Descripcion == valorCorreo).FirstOrDefaultAsync();
            seguridad _seguridad = new seguridad();
            String valor = _seguridad.Encrypt("email:" + usr.Alias + "-" + "solicitud:" + System.DateTime.Now.Ticks.ToString(), seguridad.appPwdUnique, int.Parse("256"));
            String sniffer = HttpUtility.UrlEncode(valor);

            foreach (UsuarioMail t in MailsEmpleado)
            {
                if (Cuerpocorreo != null)
                {
                    String body = String.Format(Cuerpocorreo.Texto, usr.PrimerNombre + " " + usr.PrimerApellido, sniffer);
                    seguridad.enviarCorreo(t.Completo, body, "Bienvenida Pulli");
                }
            }
            return 1;
        }

        [HttpPost("spEnviarGiftCard")]
        public async Task<clsRespuesta> enviarGiftCard(string nombre, string correo, String monto)
        {
            clsRespuesta objRespuesta = new clsRespuesta();
            try
            {
                OutputParameter<short?> resultado = new OutputParameter<short?>();
                var textoMail = await _context.TextosMail.Where(x => x.Descripcion == "GIFTCARD").FirstOrDefaultAsync();
                String CuerpoCorreo = String.Format(textoMail.Texto, nombre);
                seguridad.enviarCorreo(correo, CuerpoCorreo, "GiftCard - Pulli ");
                List<getNumeroOrdenResult> autNumber = await _contextSP.getNumeroOrdenAsync();
                var refUUser = await fireb.obtenerUsrReference(correo);
                getNumeroOrdenResult autreference = autNumber.ElementAt<getNumeroOrdenResult>(0);
                double autorizacion = double.Parse(autreference.noOrden.ToString());
                Usuario user = _context.Usuario.Where(x => x.Alias == correo).FirstOrDefault();
                EmpresaSocia empSocia = await _context.EmpresaSocia.Where(x => x.Id == user.IdEmpresa).FirstOrDefaultAsync();

                //   fireb.agregarTransaccion("credito", monto, "GiftCard - Empleado Mes", empSocia.Direccion, "credito", autorizacion.ToString(), refUUser.Value.ToString());
                objRespuesta.resultado = true;

            }
            catch (Exception e)
            {
                objRespuesta.mensaje = "No se pudo completar la transaccion intente mas tarde";
                objRespuesta.resultado = false;
            }
            return objRespuesta;
        }

        #region campana

        [HttpPost("crearCampana")]
        public async Task<clsRespuesta> crearCampana([FromBody] Campana objCampana) {
            clsRespuesta respuesta = new clsRespuesta();
            objCampana.FechaCreacion = System.DateTime.Now;
            try
            {
                _context.Campana.Add(objCampana);
                await _context.SaveChangesAsync();
                respuesta.resultado = true;
                respuesta.data = objCampana.Id;
            }
            catch (Exception e) {
                respuesta.resultado = false;
                respuesta.mensaje = "No se pudo crear la Campana";
            }
            return respuesta;
        }
        [HttpPost("guardarInvitacion")]
        public async Task<clsRespuesta> guardarInvitacion([FromBody] InvitacionesPendientes invitacion)
        {
            clsRespuesta respuesta = new clsRespuesta();
            invitacion.FechaEnvio = System.DateTime.Now;
            try
            {
                _context.InvitacionesPendientes.Add(invitacion);
                await _context.SaveChangesAsync();
                if (invitacion.Telefono.Equals("0"))
                {

                }
                else {
                    decimal mont  =invitacion.Monto ?? 0;
                    enviarComunicacion(1, invitacion.Telefono, "hello_world",mont.ToString("0.00"),invitacion.Nombre);

                }
                respuesta.resultado = true;
                respuesta.data = invitacion.Id;
            }
            catch (Exception e)
            {
                respuesta.resultado = false;
                respuesta.mensaje = "No se pudo crear la Campana";
            }
            return respuesta;
        }
        [HttpGet("obtenerCampanasParaAutorizar")]
        public async Task<clsRespuesta> obtenerCampanasAutorizar()
        {
            clsRespuesta respuesta = new clsRespuesta();
            respuesta.resultado = true;
            List<Campana> campanas = await _context.Campana.Where(x => x.Estado == 2).ToListAsync();
            respuesta.data = campanas;
            return respuesta;

        }


        [HttpGet("obtenerTarjetasSinFiltro")]
        public async Task<clsRespuesta> obtenerTarjetasRegalosNoFiltro() {
            clsRespuesta respuesta = new clsRespuesta();
            respuesta.resultado = true;
            List<TarjetaRegalo> tj = await _context.TarjetaRegalo.ToListAsync();
            respuesta.data = tj;
            return respuesta;

        }
        [HttpGet("ActualizarEstadoCampana")]
        public async Task<clsRespuesta> obtenerTarjetasRegalosNoFiltro(long id, int estado, int imagen)
        {
            clsRespuesta respuesta = new clsRespuesta();
            Campana campana = await _context.Campana.Where(x => x.Id == id).FirstAsync();
            campana.Estado = estado;
            campana.IdTarjetaRegalo = imagen;
            _context.Campana.Update(campana);
            _context.SaveChanges();
            respuesta.resultado = true;
            return respuesta;

        }

        [HttpGet("AgregarTransaccionCampana")]
        public async Task<clsRespuesta> agregarTransaccionCampana(int idCampana, String correo, decimal monto)
        {
            clsRespuesta respuesta = new clsRespuesta();
            TransaccionesCampana transaccion = new TransaccionesCampana();
            transaccion.Correo = correo;
            transaccion.Estado = 0;
            transaccion.IdCampana = idCampana;
            transaccion.Monto = monto;
            _context.TransaccionesCampana.Add(transaccion);
            try
            {
                _context.SaveChanges();
                respuesta.resultado = true;
            }
            catch (Exception e) {
                respuesta.resultado = false;
            }

            return respuesta;

        }

        [HttpGet("AgregarPreAutorizacionCampana")]
        public async Task<clsRespuesta> preautorizacionCampana(int idCampana)
        {
            clsRespuesta respuesta = new clsRespuesta();
            Campana campana = await _context.Campana.Where(x => x.Id == idCampana).FirstAsync();
            campana.Estado = 2;
            //            campana.IdTarjetaRegalo = imagen;
            EmpresaSocia socia = await _context.EmpresaSocia.Where(x => x.Id == campana.IdEmpresa).FirstAsync();

            _context.Campana.Update(campana);
            _context.SaveChanges();


            respuesta.resultado = true;
            var textoCorreo = await _context.TextosMail.Where(x => x.Descripcion == "PREAUTORIZACION").FirstOrDefaultAsync();
            String bodyCorreo = String.Format(textoCorreo.Texto, socia.Nombre, campana.Nombre);
            seguridad.enviarCorreo("WILLBATZ@GMAIL.COM", bodyCorreo, "Autorizar Campaña");


            return respuesta;


        }

        [HttpGet("AnularCampana")]
        public async Task<clsRespuesta> anularCampana(int idCampana) {
            clsRespuesta rep = new clsRespuesta();
            Campana campana = await _context.Campana.Where(x => x.Id == idCampana).FirstAsync();
            List<TransaccionesCampana> t = await _context.TransaccionesCampana.Where(x => x.IdCampana == idCampana).ToListAsync();

            campana.Estado = -1;
            var a = _context.Campana.Update(campana);
            _context.SaveChanges();
            if (campana.TipoCliente == 2)
            {

            }
            else {
                foreach (TransaccionesCampana tc in t)
                {
                    Usuario user = await _context.Usuario.Where(x => x.Alias == tc.Correo).FirstOrDefaultAsync();
                    var token = await fireb.obtenerTokenUsuario(tc.Correo);
                    await fireb.enviarrNotificacion("Notificacion", "No hemos podido validar tu operacion #"+campana.NumeroDocumento+"por Q" + tc.Monto.ToString()+". Cualquier duda Marca 4567", token.Value);

                }

            }
            return rep;

        }


        [HttpGet("AutorizarCampana")]
        public async Task<clsRespuesta> autorizarCampana(int idCampana)
        {
            clsRespuesta respuesta = new clsRespuesta();
            Campana campana = await _context.Campana.Where(x => x.Id == idCampana).FirstAsync();
            campana.Estado = 3;
            EmpresaSocia socia = await _context.EmpresaSocia.Where(x => x.Id == campana.IdEmpresa).FirstAsync();
            List<TransaccionesCampana> t = await _context.TransaccionesCampana.Where(x => x.IdCampana == idCampana).ToListAsync();

            if (campana.TipoCliente == 2)
            {
               
                //            campana.IdTarjetaRegalo = imagen;
                

                _context.Campana.Update(campana);
                _context.SaveChanges();

                foreach (TransaccionesCampana tc in t)
                {
                    var tokenUser = await fireb.obtenerUsrReference(tc.Correo);
                    fireb.agregarTransaccion("credito", tc.Monto.ToString(), "Acreditacion: " + campana.Nombre, "AdmPulli", "Acreditacion Masiva", tc.Id.ToString(), tokenUser.Value);
                    var token = await fireb.obtenerTokenUsuario(tc.Correo);
                    fireb.enviarrNotificacion("Notificacion", "Has recibido una acreditacion por: Q" + tc.Monto.ToString(), token.Value);
                }
                respuesta.resultado = true;
                var textoCorreo = await _context.TextosMail.Where(x => x.Descripcion == "PREAUTORIZACION").FirstOrDefaultAsync();
                String bodyCorreo = String.Format(textoCorreo.Texto, socia.Nombre, campana.Nombre);
                //  seguridad.enviarCorreo("WILLBATZ@GMAIL.COM", bodyCorreo, "Autorizar Campaña");

            }
            else {
                operaciones op = new operaciones();
                 foreach (TransaccionesCampana tc in t)
                {
                    Usuario user = await _context.Usuario.Where(x => x.Alias == tc.Correo).FirstOrDefaultAsync();
                    UsuarioCuenta cuenta = await _context.UsuarioCuenta.Where(x => x.IdUsuario==user.Id).Where(x=>x.Llave=="CORRIENTE").FirstOrDefaultAsync();
                    UsuarioMovimiento movUser = op.crearCredito(user.Id,(decimal)tc.Monto,cuenta.Id,"admInter","Acreditacion ACH","Acreditacion ACH","46456");
                    var token = await fireb.obtenerTokenUsuario(tc.Correo);
                    var a =await _context.UsuarioMovimiento.AddAsync(movUser);
                    cuenta.Saldo = cuenta.Saldo + (decimal)tc.Monto;
                    _context.UsuarioCuenta.Update(cuenta);
                   await fireb.enviarrNotificacion("Notificacion", "Has recibido una acreditacion por: Q" + tc.Monto.ToString(), token.Value);

                }
                _context.Campana.Update(campana);

                _context.SaveChanges();
                respuesta.resultado = true;
                
            }
            return respuesta;

        }

        [HttpPost("EnviarWhatsapp")]
        public async Task<clsRespuesta> enviarComunicacion(int canal, string telefono, string plantilla,string monto,string nombre)
        {
            clsRespuesta respuesta = new clsRespuesta();
            if (canal == 1)
            {
                clsSMS.Enviar("502", telefono, "Hola " + nombre + " ,Tiendas Mass premia tu lealtad descarga la APP en https://bit.ly/42d36W1 y obtendras Q"+monto);

                var options = new RestClientOptions("https://graph.facebook.com")
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest("/v18.0/207872892409779/messages", Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer EAAWRSDjVQmQBO9txpOfratsvCXm5Ao8gMSRyVU7ubwXnfQw7qZB4BJIxFWqDTDZBzIvuQVvR0HrOptQpRKobZChFALK0HZA8HtcjXUl2S8oMQEMXDg5V7JgSah17QZBRmGRYZAz6ZA1Hy4NXcFMPEtEmzv1PdMxjzdSlfIt3ZABt8QqPnEZBa7cXwRAXkZAKTN13cIISaeWv1YzA0lQQhGkP7W6YuXZCEBMoSaoj5kZCMVlp0K0y");
                var body = "{ 'messaging_product': 'whatsapp', 'to': '" + telefono + "', 'type': 'template', 'template': { 'name': 'premiamos_tu_fidelidad', 'language': { 'code': 'es_MX' } } }";
                request.AddStringBody(body, DataFormat.Json);
                RestResponse response = await client.ExecuteAsync(request);
                Console.WriteLine(response.Content);
                respuesta.resultado = true;
            }   
            
            else {

            }
            return respuesta;

        }
        #endregion


    }
}

