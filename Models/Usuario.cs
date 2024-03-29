﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace apiBillFold.Models
{
    public partial class Usuario
    {
        public long Id { get; set; }
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? OtrosNombres { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public string? ApellidoCasada { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Codigoempleado { get; set; } = null!;
        public string Nit { get; set; } = null!;
        public decimal Salario { get; set; }
        public decimal Disponible { get; set; }
        public string Dpi { get; set; } = null!;
        public string Alias { get; set; } = null!;
        public int Estado { get; set; }
        public long IdEmpresa { get; set; }
        public byte[]? Contrasena { get; set; }
    }
}