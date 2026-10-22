using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Models;

public partial class HotelContext : DbContext
{
    public HotelContext()
    {
    }

    public HotelContext(DbContextOptions<HotelContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Habitacion> Habitacions { get; set; }

    public virtual DbSet<Huesped> Huespeds { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<TipoHabitacion> TipoHabitacions { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Habitacion>(entity =>
        {
            entity.HasKey(e => e.IdHabitacion).HasName("PK__Habitaci__8BBBF9012F5602AB");

            entity.ToTable("Habitacion");

            entity.HasIndex(e => e.Numero, "UQ__Habitaci__7E532BC61F2387B2").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Numero).HasMaxLength(10);

            entity.HasOne(d => d.IdTipoNavigation).WithMany(p => p.Habitacions)
                .HasForeignKey(d => d.IdTipo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Habitacio__IdTip__5070F446");
        });

        modelBuilder.Entity<Huesped>(entity =>
        {
            entity.HasKey(e => e.IdHuesped).HasName("PK__Huesped__939EC061A87B3607");

            entity.ToTable("Huesped");

            entity.HasIndex(e => e.Documento, "UQ__Huesped__AF73706D9EC0F0D3").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Apellidos).HasMaxLength(80);
            entity.Property(e => e.Documento).HasMaxLength(15);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Nombres).HasMaxLength(80);
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.IdReserva).HasName("PK__Reserva__0E49C69DAFAB0098");

            entity.ToTable("Reserva");

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Observaciones).HasMaxLength(200);

            entity.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdHabitacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reserva__IdHabit__59063A47");

            entity.HasOne(d => d.IdHuespedNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdHuesped)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reserva__IdHuesp__5812160E");
        });

        modelBuilder.Entity<TipoHabitacion>(entity =>
        {
            entity.HasKey(e => e.IdTipo).HasName("PK__TipoHabi__9E3A29A530BCE922");

            entity.ToTable("TipoHabitacion");

            entity.HasIndex(e => e.Nombre, "UQ__TipoHabi__75E3EFCF27D6B5A6").IsUnique();

            entity.Property(e => e.Nombre).HasMaxLength(50);
            entity.Property(e => e.PrecioNoche).HasColumnType("decimal(10, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
