using MovilL.Models;
using MovilL.Services;

namespace MovilL.views;

public partial class LoginPage : ContentPage
{
    private readonly IMovilDatabaseService _databaseService;

    public LoginPage(IMovilDatabaseService databaseService)
    {
        _databaseService = databaseService;
        InitializeComponent();
    }

    private async void OnIniciarSesionClicked(object? sender, EventArgs e)
    {
        string userText = EntryUsuario.Text?.Trim() ?? string.Empty;
        string passText = EntryContrasena.Text?.Trim() ?? string.Empty;

        Usuario? usuario;
        try
        {
            usuario = await _databaseService.GetUsuarioPorCredencialesAsync(userText, passText);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudo consultar el usuario: {ex.Message}");
            LabelError.Text = "No se pudo validar la cuenta. Inténtalo de nuevo.";
            LabelError.IsVisible = true;
            return;
        }

        if (usuario != null)
        {
            Usuario.ActualizarUsuario(usuario);
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
            LabelError.Text = "Usuario o contraseña incorrectos";
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