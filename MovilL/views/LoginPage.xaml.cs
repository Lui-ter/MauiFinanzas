using MovilL.Models;

namespace MovilL.views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnIniciarSesionClicked(object? sender, EventArgs e)
    {
        string userText = EntryUsuario.Text?.Trim() ?? string.Empty;
        string passText = EntryContrasena.Text?.Trim() ?? string.Empty;

        var usuario = Usuario.ValidarCredenciales(userText, passText);

        if (usuario != null)
        {
            Sesion.CerrarSesion();
            Sesion.EstaLogueado = true;
            Sesion.EsInvitado = false;
            Sesion.NombreUsuario = usuario.Nombre;
            Sesion.UsuarioActual = usuario;

            LabelError.IsVisible = false;
            await Shell.Current.GoToAsync("//Inicio");
        }
        else
        {
            LabelError.IsVisible = true;
        }
    }

    private async void OnEntrarInvitadoClicked(object? sender, EventArgs e)
    {
        Sesion.CerrarSesion();
        Sesion.EstaLogueado = true;
        Sesion.EsInvitado = true;
        Sesion.NombreUsuario = "Invitado";
        Sesion.UsuarioActual = new Usuario
        {
            Id = 0,
            Nombre = "Invitado",
            NombreUsuario = "invitado",
            Contrasena = "",
            Edad = 0,
            Correo = "invitado@movill.com",
            Imagen = "user.png"
        };

        await Shell.Current.GoToAsync("//Inicio");
    }
}