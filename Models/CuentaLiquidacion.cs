﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace apiBillFold.Models
{
    public partial class CuentaLiquidacion
    {
        public int Id { get; set; }
        public int? IdEmpresa { get; set; }
        public int? IdBanco { get; set; }
        public string? NumeroCuenta { get; set; }
        public int? IdTipo { get; set; }
        public string? Iban { get; set; }
    }
}