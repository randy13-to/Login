using Microsoft.EntityFrameworkCore;

namespace LoginApi.Models;

public partial class LoginContext : DbContext
{
    public LoginContext()
    {
    }

    public LoginContext(DbContextOptions<LoginContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("Usuarios");

            entity.HasIndex(e => e.Correo, "UQ_Usuarios_Correo").IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.Property(e => e.Rol)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Usuario");

            entity.Property(e => e.Activo)
                .HasDefaultValue(true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}