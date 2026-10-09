namespace MovilL.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Edad { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Imagen { get; set; } = "user.png";
    public string NombreUsuario { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;

    // Lista de usuarios precargados en memoria
    public static List<Usuario> ListaUsuarios = new()
    {
        new Usuario
        {
            Id = 1,
            Nombre = "Administrador",
            NombreUsuario = "admin",
            Contrasena = "1234",
            Edad = 28,
            Correo = "admin@movill.com",
            Imagen = "user.png"
        },
        new Usuario
        {
            Id = 2,
            Nombre = "Carlos Mendoza",
            NombreUsuario = "carlos",
            Contrasena = "1234",
            Edad = 24,
            Correo = "carlos@movill.com",
            Imagen = "user.png"
        }
    };

    public static Usuario? ValidarCredenciales(string usuario, string contrasena)
    {
        return ListaUsuarios.FirstOrDefault(u =>
            u.NombreUsuario.Equals(usuario, StringComparison.OrdinalIgnoreCase) &&
            u.Contrasena == contrasena);
    }
}

