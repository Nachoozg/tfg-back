using System;
using System.Collections.Generic;

namespace ligaTenisBack.Models.DbModels;

public partial class Rol
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
