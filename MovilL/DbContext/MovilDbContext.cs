using Microsoft.EntityFrameworkCore;
using MovilL.Models;

namespace MovilL.DbContext;

public class MovilDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public MovilDbContext(DbContextOptions<MovilDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasKey(u => u.Id);
        modelBuilder.Entity<Usuario>().Property(u => u.NombreUsuario).IsRequired();
        modelBuilder.Entity<Usuario>().Property(u => u.Contrasena).IsRequired();

        modelBuilder.Entity<Categoria>().HasKey(c => c.Pk);
        modelBuilder.Entity<Categoria>().Ignore(c => c.Id);
        modelBuilder.Entity<Categoria>().HasIndex(c => c.UsuarioId);
        modelBuilder.Entity<Categoria>().HasData(Categoria.ListaCategorias);

        modelBuilder.Entity<Movimiento>().HasKey(m => m.Id);
        modelBuilder.Entity<Movimiento>().Property(m => m.Monto).HasPrecision(18, 2);
        modelBuilder.Entity<Movimiento>().HasIndex(m => m.UsuarioId);

        modelBuilder.Entity<Usuario>().HasData(Usuario.ListaUsuarios);
    }
}
