using MovilL.Models;

namespace MovilL.Services;

public interface IMovilDatabaseService
{
    Task EnsureCreatedAsync();
    Task<List<Movimiento>> GetMovimientosAsync(int usuarioId);
    Task GuardarMovimientoAsync(Movimiento movimiento);
    Task<List<Categoria>> GetCategoriasAsync(int usuarioId);
    Task<bool> ExisteCategoriaAsync(string nombre, string tipo, int usuarioId);
    Task<Categoria> GuardarCategoriaAsync(Categoria categoria);
    Task<Categoria> ActualizarCategoriaAsync(Categoria categoria, int usuarioId);
    Task<bool> EliminarCategoriaAsync(int categoriaId, int usuarioId);
    Task<Usuario?> GetUsuarioPorCredencialesAsync(string nombreUsuario, string contrasena);
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int idExcluir = 0);
    Task<Usuario> GuardarUsuarioAsync(Usuario usuario);
    Task EliminarUsuarioAsync(int usuarioId);
}
