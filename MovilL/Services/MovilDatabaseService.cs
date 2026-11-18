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
    }

    public async Task<List<Movimiento>> GetMovimientosAsync(int usuarioId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Movimientos.AsNoTracking();

        if (usuarioId > 0)
            query = query.Where(m => m.UsuarioId == usuarioId);

        return await query.OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Id).ToListAsync();
    }

    public async Task GuardarMovimientoAsync(Movimiento movimiento)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Movimientos.Add(movimiento);
        await context.SaveChangesAsync();
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
            context.Usuarios.Remove(usuario);
            await context.SaveChangesAsync();
        }
    }
}
