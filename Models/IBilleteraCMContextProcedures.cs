﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using apiBillFold.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace apiBillFold.Models
{
    public partial interface IBilleteraCMContextProcedures
    {
        Task<int> actualizarAutorizacionAsync(long? idmodificar, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<int> agregarAdministradorAsync(string pNombre, string sNombre, string pApellido, string sApellido, long? dpi, string user, int? idEmpresa, string contra, int? tipo, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<int> agregarUsuarioAsync(string pNombre, string sNombre, string oNombre, string pApellido, string sApellido, string cApellido, string fechaNacimiento, string codiogEmpleado, string nit, decimal? salario, decimal? disponible, string dpi, string user, int? estado, long? idEmpresa, string hash, OutputParameter<long?> idEmpleado, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<List<buscarClienteResult>> buscarClienteAsync(string telefono, string mail, string nit, int? tipoBusqueda, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<int> crearClienteAsync(string primerNombre, string segundoNombre, string otrosNombre, string primerApellico, string segundoApellido, string apelllidoCasada, string nit, string alias, long? idempresa, int? telefono, OutputParameter<long?> resultado, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<int> generarTokenAsync(string mail, decimal? monto, int? comercio, OutputParameter<int?> resultado, OutputParameter<decimal?> token, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<List<getAsociacionDetallePaisResult>> getAsociacionDetallePaisAsync(int? idPais, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<List<getNumeroOrdenResult>> getNumeroOrdenAsync(OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<List<getTokenByMailResult>> getTokenByMailAsync(string correo, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<List<ManejoIngresoResult>> ManejoIngresoAsync(string Usuario, string password, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<List<ManejoIngresoEmpleadoResult>> ManejoIngresoEmpleadoAsync(string Usuario, string password, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<List<obtenerMenuResult>> obtenerMenuAsync(long? idUsuario, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<List<obtenerMenuAdministradorResult>> obtenerMenuAdministradorAsync(OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<int> realizarTransaccionAsync(int? empresaSocia, decimal? monto, int? tipoTransaccion, string documento, string descripcion, long? idUsuario, long? idoperador, OutputParameter<long?> idTransaccion, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<int> updateContrasenaAsync(string contra, long? id, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<int> updateContrasenaEmpleadoAsync(string contra, long? id, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<int> validarClienteAsync(string telefono, string mail, string dpi, long? idEmpresa, OutputParameter<string> Mensaje, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<int> validarExistenciaUsuarioAsync(string correo, OutputParameter<int?> resultado, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<int> validarTokenAsync(long? usuario, decimal? token, OutputParameter<short?> resultado, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
    }
}