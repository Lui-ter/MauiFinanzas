namespace MovilL.views;

public partial class LoginPage : ContentPage
{
    // Credenciales quemadas para esta etapa del proyecto
    private const string UsuarioValido = "admin";
    private const string ContrasenaValida = "1234";

    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnIniciarSesionClicked(object? sender, EventArgs e)
    {
        if (EntryUsuario.Text == UsuarioValido && EntryContrasena.Text == ContrasenaValida)
        {
            Sesion.CerrarSesion();
            Sesion.EstaLogueado = true;
            Sesion.EsInvitado = false;
            Sesion.NombreUsuario = UsuarioValido;

            await Shell.Current.GoToAsync("//NuevoMovimientoPage");
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

        await Shell.Current.GoToAsync("//NuevoMovimientoPage");
    }
}