﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace apiBillFold.Models
{
    public partial class Autorizacion
    {
        public long Id { get; set; }
        public long IdUsuario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public long Valor { get; set; }
        public short Estado { get; set; }
    }
}