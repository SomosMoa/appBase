﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace apiBillFold.Models
{
    public partial class MovimientoDiario
    {
        public long Id { get; set; }
        public long Idempleado { get; set; }
        public DateTime? Fecha { get; set; }
        public long Tipo { get; set; }
        public decimal Monto { get; set; }
        public long Documento { get; set; }
        public string? Descripcion { get; set; }
        public int? TipoMovimiento { get; set; }
        public long? IdEmpresa { get; set; }
    }
}