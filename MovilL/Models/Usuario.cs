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
            Nombre = "Mauricio Rodriguez",
            NombreUsuario = "mauricio",
            Contrasena = "1234",
            Edad = 21,
            Correo = "mauricio@movill.com",
            Imagen = "npc.jpg"
        }
    };

    public static Usuario? ValidarCredenciales(string usuario, string contrasena)
    {
        return ListaUsuarios.FirstOrDefault(u =>
            u.NombreUsuario.Equals(usuario, StringComparison.OrdinalIgnoreCase) &&
            u.Contrasena == contrasena);
    }

    public static bool ActualizarUsuario(Usuario usuarioActualizado)
    {
        var index = ListaUsuarios.FindIndex(u => u.Id == usuarioActualizado.Id);
        if (index != -1)
        {
            ListaUsuarios[index].Nombre = usuarioActualizado.Nombre;
            ListaUsuarios[index].NombreUsuario = usuarioActualizado.NombreUsuario;
            ListaUsuarios[index].Correo = usuarioActualizado.Correo;
            ListaUsuarios[index].Edad = usuarioActualizado.Edad;
            ListaUsuarios[index].Contrasena = usuarioActualizado.Contrasena;
            ListaUsuarios[index].Imagen = usuarioActualizado.Imagen;
            return true;
        }
        else
        {
            if (usuarioActualizado.Id <= 0)
            {
                int nuevoId = ListaUsuarios.Count > 0 ? ListaUsuarios.Max(u => u.Id) + 1 : 1;
                usuarioActualizado.Id = nuevoId;
            }
            ListaUsuarios.Add(usuarioActualizado);
            return true;
        }
    }

    public static bool EliminarUsuario(int id)
    {
        var usuario = ListaUsuarios.FirstOrDefault(u => u.Id == id);
        if (usuario != null)
        {
            return ListaUsuarios.Remove(usuario);
        }
        return false;
    }

    public static bool ExisteNombreUsuario(string nombreUsuario, int idExcluir = 0)
    {
        return ListaUsuarios.Any(u =>
            u.Id != idExcluir &&
            u.NombreUsuario.Equals(nombreUsuario, StringComparison.OrdinalIgnoreCase));
    }
}

