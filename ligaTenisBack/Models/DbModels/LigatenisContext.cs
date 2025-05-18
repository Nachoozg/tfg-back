using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace ligaTenisBack.Models.DbModels;

public partial class LigatenisContext : DbContext
{
    public LigatenisContext()
    {
    }

    public LigatenisContext(DbContextOptions<LigatenisContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Colegio> Colegios { get; set; }

    public virtual DbSet<Jugador> Jugadors { get; set; }

    public virtual DbSet<Partido> Partidos { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Colegio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("colegio");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ImagenColegio)
                .HasMaxLength(250)
                .HasColumnName("imagenColegio");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.NumeroJugadores).HasColumnName("numeroJugadores");
        });

        modelBuilder.Entity<Jugador>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("jugador");

            entity.HasIndex(e => e.ColegioId, "colegio_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(50)
                .HasColumnName("apellidos");
            entity.Property(e => e.ColegioId).HasColumnName("colegio_id");
            entity.Property(e => e.Edad)
                .HasMaxLength(50)
                .HasColumnName("edad");
            entity.Property(e => e.ImagenJugador)
                .HasMaxLength(250)
                .HasColumnName("imagenJugador");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");

            entity.HasOne(d => d.Colegio).WithMany(p => p.Jugadors)
                .HasForeignKey(d => d.ColegioId)
                .HasConstraintName("FK_jugador_colegio");
        });

        modelBuilder.Entity<Partido>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("partido");

            entity.HasIndex(e => e.LocalId, "FK_partido_colegio");

            entity.HasIndex(e => e.VisitanteId, "FK_partido_colegio_2");

            entity.HasIndex(e => e.JugadorLocalId, "FK_partido_jugador");

            entity.HasIndex(e => e.JugadorVisitanteId, "FK_partido_jugador_2");

            entity.HasIndex(e => new { e.Fecha, e.LocalId, e.VisitanteId }, "fecha_local_id_visitante_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Detalles)
                .HasMaxLength(50)
                .HasColumnName("detalles");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.JugadorLocalId).HasColumnName("jugador_local_id");
            entity.Property(e => e.JugadorVisitanteId).HasColumnName("jugador_visitante_id");
            entity.Property(e => e.Lat).HasColumnName("lat");
            entity.Property(e => e.Lng).HasColumnName("lng");
            entity.Property(e => e.LocalId).HasColumnName("local_id");
            entity.Property(e => e.Lugar)
                .HasMaxLength(50)
                .HasColumnName("lugar");
            entity.Property(e => e.ResultadoLocal).HasColumnName("resultado_local");
            entity.Property(e => e.ResultadoVisitante).HasColumnName("resultado_visitante");
            entity.Property(e => e.VisitanteId).HasColumnName("visitante_id");

            entity.HasOne(d => d.JugadorLocal).WithMany(p => p.PartidoJugadorLocals)
                .HasForeignKey(d => d.JugadorLocalId)
                .HasConstraintName("FK_partido_jugador");

            entity.HasOne(d => d.JugadorVisitante).WithMany(p => p.PartidoJugadorVisitantes)
                .HasForeignKey(d => d.JugadorVisitanteId)
                .HasConstraintName("FK_partido_jugador_2");

            entity.HasOne(d => d.Local).WithMany(p => p.PartidoLocals)
                .HasForeignKey(d => d.LocalId)
                .HasConstraintName("FK_partido_colegio");

            entity.HasOne(d => d.Visitante).WithMany(p => p.PartidoVisitantes)
                .HasForeignKey(d => d.VisitanteId)
                .HasConstraintName("FK_partido_colegio_2");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("rol");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("usuario");

            entity.HasIndex(e => e.RolId, "FK_usuario_rol");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(50)
                .HasColumnName("apellidos");
            entity.Property(e => e.Aprobado).HasColumnName("aprobado");
            entity.Property(e => e.Mail)
                .HasMaxLength(50)
                .HasColumnName("mail");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.Password)
                .HasMaxLength(65)
                .HasColumnName("password");
            entity.Property(e => e.RolId).HasColumnName("rolId");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .HasConstraintName("FK_usuario_rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
