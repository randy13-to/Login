using Microsoft.EntityFrameworkCore;

namespace LoginApi.Models;

public class LoginContext : DbContext
{
    public LoginContext(DbContextOptions<LoginContext> options) : base(options)
    {
    }

    public virtual DbSet<Usuario> Usuarios { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios", "dbo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Contrasena).HasColumnName("ContrasenaHash");
        });
    }
}