using System;
using System.Collections.Generic;

namespace ligaTenisBack.Models.DbModels;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Mail { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? RolId { get; set; }

    public sbyte? Aprobado { get; set; }

    public virtual Rol? Rol { get; set; }
}
