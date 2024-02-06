using System;
using apiBillFold.Models;
using System.Security.Claims;

namespace apiBillFold.Externo
{
    public class jwConfig
    {
        public string key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Subject { get; set; }

        public static Usuario validarToken(ClaimsIdentity identity, BilleteraCMContext _context)
        {
            try
            {
                if (identity.Claims.Count() == 0)
                {
                    return null;
                }
                var id = identity.Claims.FirstOrDefault(x => x.Type == "id").Value;
                long idbuscar = long.Parse(id);
                Usuario usuarioEncontrado = _context.Usuario.Where(x => x.Id == idbuscar).FirstOrDefault();
                return usuarioEncontrado;
            }

            catch (Exception e)
            {
                return null;

            }
        }

    }
}

