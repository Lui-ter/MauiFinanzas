using Microsoft.EntityFrameworkCore;
using MovilL.DbContext;
using MovilL.Models;

namespace MovilL.Services;

public class MovilDatabaseService : IMovilDatabaseService
{
    private readonly IDbContextFactory<MovilDbContext> _contextFactory;

    public MovilDatabaseService(IDbContextFactory<MovilDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task EnsureCreatedAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await context.Database.EnsureCreatedAsync();

        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        try
        {
            bool tieneUsuarioId = false;
            await using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA table_info('Categorias')";
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (reader.GetString(1).Equals("UsuarioId", StringComparison.OrdinalIgnoreCase))
                    {
                        tieneUsuarioId = true;
                        break;
                    }
                }
            }

            if (!tieneUsuarioId)
            {
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE \"Categorias\" ADD COLUMN \"UsuarioId\" INTEGER NOT NULL DEFAULT 0");
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    public async Task<List<Movimiento>> GetMovimientosAsync(int usuarioId)
    {
        if (usuarioId <= 0)
            return new List<Movimiento>();

        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Movimientos
            .AsNoTracking()
            .Where(m => m.UsuarioId == usuarioId)
            .OrderByDescending(m => m.Fecha)
            .ThenByDescending(m => m.Id)
            .ToListAsync();
    }

    public async Task GuardarMovimientoAsync(Movimiento movimiento)
    {
        if (movimiento.UsuarioId <= 0)
            throw new InvalidOperationException("Solo una cuenta registrada puede guardar movimientos en SQLite.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Movimientos.Add(movimiento);
        await context.SaveChangesAsync();
    }

    public async Task<List<Categoria>> GetCategoriasAsync(int usuarioId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Categorias
            .AsNoTracking()
            .Where(c => c.UsuarioId == 0 || (usuarioId > 0 && c.UsuarioId == usuarioId))
            .OrderBy(c => c.Pk)
            .ToListAsync();
    }

    public async Task<bool> ExisteCategoriaAsync(string nombre, string tipo, int usuarioId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var nombreNormalizado = nombre.Trim().ToLowerInvariant();

        return await context.Categorias.AnyAsync(c =>
            c.Tipo == tipo &&
            (c.UsuarioId == 0 || c.UsuarioId == usuarioId) &&
            c.Nombre.ToLower() == nombreNormalizado);
    }

    public async Task<Categoria> GuardarCategoriaAsync(Categoria categoria)
    {
        if (categoria.UsuarioId <= 0)
            throw new InvalidOperationException("Las categorías personalizadas requieren una cuenta registrada.");

        categoria.Nombre = categoria.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(categoria.Nombre))
            throw new InvalidOperationException("El nombre de la categoría es obligatorio.");
        if (categoria.Tipo is not ("Ingreso" or "Gasto"))
            throw new InvalidOperationException("El tipo de categoría debe ser Ingreso o Gasto.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var nombreNormalizado = categoria.Nombre.ToLowerInvariant();
        bool existe = await context.Categorias.AnyAsync(c =>
            c.Tipo == categoria.Tipo &&
            (c.UsuarioId == 0 || c.UsuarioId == categoria.UsuarioId) &&
            c.Nombre.ToLower() == nombreNormalizado);
        if (existe)
            throw new InvalidOperationException("Ya existe una categoría con ese nombre para este tipo.");

        categoria.Pk = 0;
        context.Categorias.Add(categoria);
        await context.SaveChangesAsync();
        return categoria;
    }

    public async Task<Usuario?> GetUsuarioPorCredencialesAsync(string nombreUsuario, string contrasena)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.NombreUsuario.ToLower() == nombreUsuario.ToLower() &&
                u.Contrasena == contrasena);
    }

    public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int idExcluir = 0)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var nombreNormalizado = nombreUsuario.ToLower();

        return await context.Usuarios.AnyAsync(u =>
            u.Id != idExcluir &&
            u.NombreUsuario.ToLower() == nombreNormalizado);
    }

    public async Task<Usuario> GuardarUsuarioAsync(Usuario usuario)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        if (usuario.Id > 0)
            context.Usuarios.Update(usuario);
        else
            context.Usuarios.Add(usuario);

        await context.SaveChangesAsync();
        return usuario;
    }

    public async Task EliminarUsuarioAsync(int usuarioId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios.FindAsync(usuarioId);
        if (usuario != null)
        {
            context.Movimientos.RemoveRange(context.Movimientos.Where(m => m.UsuarioId == usuarioId));
            context.Categorias.RemoveRange(context.Categorias.Where(c => c.UsuarioId == usuarioId));
            context.Usuarios.Remove(usuario);
            await context.SaveChangesAsync();
        }
    }
}
