using MovilL.Models;

namespace MovilL.Services;

public interface IMovilDatabaseService
{
    Task EnsureCreatedAsync();
    Task<List<Movimiento>> GetMovimientosAsync(int usuarioId);
    Task GuardarMovimientoAsync(Movimiento movimiento);
    Task<Usuario?> GetUsuarioPorCredencialesAsync(string nombreUsuario, string contrasena);
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int idExcluir = 0);
    Task<Usuario> GuardarUsuarioAsync(Usuario usuario);
    Task EliminarUsuarioAsync(int usuarioId);
}
