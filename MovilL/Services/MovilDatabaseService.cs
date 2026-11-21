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
            var columnas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA table_info('Categorias')";
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    columnas.Add(reader.GetString(1));
            }

            if (!columnas.Contains("UsuarioId"))
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE \"Categorias\" ADD COLUMN \"UsuarioId\" INTEGER NOT NULL DEFAULT 0");
            if (!columnas.Contains("CategoriaPredeterminadaId"))
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE \"Categorias\" ADD COLUMN \"CategoriaPredeterminadaId\" INTEGER NULL");
            if (!columnas.Contains("OcultaParaUsuario"))
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE \"Categorias\" ADD COLUMN \"OcultaParaUsuario\" INTEGER NOT NULL DEFAULT 0");
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
        var categorias = await GetCategoriasVisiblesAsync(context, usuarioId);
        return categorias
            .OrderBy(c => c.Pk)
            .ToList();
    }

    public async Task<bool> ExisteCategoriaAsync(string nombre, string tipo, int usuarioId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var nombreNormalizado = nombre.Trim().ToLowerInvariant();
        var categorias = await GetCategoriasVisiblesAsync(context, usuarioId);

        return categorias.Any(c =>
            c.Tipo == tipo &&
            c.Nombre.ToLowerInvariant() == nombreNormalizado);
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
        var visibles = await GetCategoriasVisiblesAsync(context, categoria.UsuarioId);
        bool existe = visibles.Any(c =>
            c.Tipo == categoria.Tipo &&
            c.Nombre.ToLowerInvariant() == nombreNormalizado);
        if (existe)
            throw new InvalidOperationException("Ya existe una categoría con ese nombre para este tipo.");

        categoria.Pk = 0;
        categoria.CategoriaPredeterminadaId = null;
        categoria.OcultaParaUsuario = false;
        context.Categorias.Add(categoria);
        await context.SaveChangesAsync();
        return categoria;
    }

    public async Task<Categoria> ActualizarCategoriaAsync(Categoria categoria, int usuarioId)
    {
        if (usuarioId <= 0 || categoria.Pk <= 0)
            throw new InvalidOperationException("No se puede actualizar una categoría predeterminada o sin propietario.");

        categoria.Nombre = categoria.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(categoria.Nombre))
            throw new InvalidOperationException("El nombre de la categoría es obligatorio.");
        if (categoria.Tipo is not ("Ingreso" or "Gasto"))
            throw new InvalidOperationException("El tipo de categoría debe ser Ingreso o Gasto.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var existente = await context.Categorias
            .FirstOrDefaultAsync(c => c.Pk == categoria.Pk && c.UsuarioId == usuarioId);
        var predeterminada = existente == null
            ? await context.Categorias.FirstOrDefaultAsync(c => c.Pk == categoria.Pk && c.UsuarioId == 0)
            : null;
        if (existente == null && predeterminada == null)
            throw new InvalidOperationException("No se encontró la categoría para actualizar.");

        int? categoriaPredeterminadaId = existente?.CategoriaPredeterminadaId ?? predeterminada?.Pk;
        if (predeterminada != null)
        {
            existente = await context.Categorias.FirstOrDefaultAsync(c =>
                c.UsuarioId == usuarioId &&
                c.CategoriaPredeterminadaId == predeterminada.Pk);
        }

        var categoriasVisibles = await GetCategoriasVisiblesAsync(context, usuarioId);
        var nombreNormalizado = categoria.Nombre.ToLowerInvariant();
        bool duplicada = categoriasVisibles.Any(c =>
            c.Pk != existente?.Pk &&
            c.Pk != categoriaPredeterminadaId &&
            c.Tipo == categoria.Tipo &&
            c.Nombre.ToLowerInvariant() == nombreNormalizado);
        if (duplicada)
            throw new InvalidOperationException("Ya existe una categoría con ese nombre para este tipo.");

        if (existente == null)
        {
            existente = new Categoria
            {
                UsuarioId = usuarioId,
                CategoriaPredeterminadaId = categoriaPredeterminadaId
            };
            context.Categorias.Add(existente);
        }

        existente.CategoriaPredeterminadaId = categoriaPredeterminadaId;
        existente.OcultaParaUsuario = false;
        existente.Nombre = categoria.Nombre;
        existente.Tipo = categoria.Tipo;
        existente.Imagen = categoria.Imagen;
        await context.SaveChangesAsync();
        return existente;
    }

    public async Task<bool> EliminarCategoriaAsync(int categoriaId, int usuarioId)
    {
        if (usuarioId <= 0 || categoriaId <= 0)
            return false;

        await using var context = await _contextFactory.CreateDbContextAsync();
        var categoria = await context.Categorias
            .FirstOrDefaultAsync(c => c.Pk == categoriaId && c.UsuarioId == usuarioId);
        if (categoria != null)
        {
            if (categoria.CategoriaPredeterminadaId.HasValue)
            {
                categoria.OcultaParaUsuario = true;
                categoria.Imagen = string.Empty;
            }
            else
            {
                context.Categorias.Remove(categoria);
            }
        }
        else
        {
            var predeterminada = await context.Categorias
                .FirstOrDefaultAsync(c => c.Pk == categoriaId && c.UsuarioId == 0);
            if (predeterminada == null)
                return false;

            var ocultacion = await context.Categorias.FirstOrDefaultAsync(c =>
                c.UsuarioId == usuarioId &&
                c.CategoriaPredeterminadaId == predeterminada.Pk);
            if (ocultacion == null)
            {
                context.Categorias.Add(new Categoria
                {
                    UsuarioId = usuarioId,
                    CategoriaPredeterminadaId = predeterminada.Pk,
                    OcultaParaUsuario = true,
                    Nombre = predeterminada.Nombre,
                    Tipo = predeterminada.Tipo
                });
            }
            else
            {
                ocultacion.OcultaParaUsuario = true;
                ocultacion.Imagen = string.Empty;
            }
        }

        await context.SaveChangesAsync();
        return true;
    }

    private static async Task<List<Categoria>> GetCategoriasVisiblesAsync(MovilDbContext context, int usuarioId)
    {
        var categorias = await context.Categorias
            .AsNoTracking()
            .Where(c => c.UsuarioId == 0 || (usuarioId > 0 && c.UsuarioId == usuarioId))
            .ToListAsync();

        var categoriasPredeterminadasPersonalizadas = categorias
            .Where(c => c.UsuarioId == usuarioId && c.CategoriaPredeterminadaId.HasValue)
            .Select(c => c.CategoriaPredeterminadaId!.Value)
            .ToHashSet();

        return categorias
            .Where(c => !c.OcultaParaUsuario &&
                        !(c.UsuarioId == 0 && categoriasPredeterminadasPersonalizadas.Contains(c.Pk)))
            .ToList();
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
