using System.Collections.Generic;
using MovilL.Models;

public static class Sesion
{
    public static bool EstaLogueado { get; set; } = false;
    public static bool EsInvitado { get; set; } = false;
    public static string NombreUsuario { get; set; } = "";

    // Lista de movimientos en memoria.
    // Si es invitado, se llena y se pierde al cerrar la app.
    // Si es usuario, más adelante esto se puede cambiar por SQLite/Preferences.
    public static List<Movimiento> Movimientos { get; set; } = new();

    public static void CerrarSesion()
    {
        EstaLogueado = false;
        EsInvitado = false;
        NombreUsuario = "";
        Movimientos.Clear();
    }
}