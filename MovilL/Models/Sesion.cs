using System.Collections.Generic;
using MovilL.Models;

public static class Sesion
{
    public static bool EstaLogueado { get; set; } = false;
    public static bool EsInvitado { get; set; } = false;
    public static string NombreUsuario { get; set; } = "";

    // Objeto de usuario actualmente autenticado o invitado
    public static Usuario? UsuarioActual { get; set; }

    // Lista de movimientos en memoria.
    public static List<Movimiento> Movimientos { get; set; } = new();

    // Lista de categorías en memoria
    public static List<Categoria> Categorias { get; set; } = Categoria.ListaCategorias;

    public static void CerrarSesion()
    {
        EstaLogueado = false;
        EsInvitado = false;
        NombreUsuario = "";
        UsuarioActual = null;
        Movimientos.Clear();
    }
}