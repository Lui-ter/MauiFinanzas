using MovilL.Models;

namespace MovilL.views;

public partial class Menu : ContentView
{
    private bool isMenuOpen = false;

    public event EventHandler? PerfilClicked;
    public event EventHandler? ConfigurarClicked;
    public event EventHandler? SesionCerrada;

    public Menu()
    {
        InitializeComponent();
        CargarUsuario();
    }

    public void CargarUsuario()
    {
        var usuario = Sesion.UsuarioActual;

        if (usuario != null)
        {
            LblNombreUsuarioMenu.Text = usuario.Nombre;
            LblCorreoUsuarioMenu.Text = string.IsNullOrWhiteSpace(usuario.Correo) ? "@" + usuario.NombreUsuario : usuario.Correo;
            string imagen = string.IsNullOrWhiteSpace(usuario.Imagen) ? "user.png" : usuario.Imagen;
            ImgUsuarioMenu.Source = ImageSource.FromFile(imagen);

            if (Sesion.EsInvitado)
            {
                LblRolUsuarioMenu.Text = "Modo Invitado";
                BadgeRol.BackgroundColor = Color.FromArgb("#3C3322");
                BadgeRol.Stroke = Color.FromArgb("#705C3D");
                LblRolUsuarioMenu.TextColor = Color.FromArgb("#FFCA28");
            }
            else
            {
                LblRolUsuarioMenu.Text = "Cuenta Principal";
                BadgeRol.BackgroundColor = Color.FromArgb("#223C2D");
                BadgeRol.Stroke = Color.FromArgb("#3D704E");
                LblRolUsuarioMenu.TextColor = Color.FromArgb("#81C784");
            }
        }
        else
        {
            LblNombreUsuarioMenu.Text = Sesion.EsInvitado ? "Invitado" : (!string.IsNullOrEmpty(Sesion.NombreUsuario) ? Sesion.NombreUsuario : "Usuario");
            LblCorreoUsuarioMenu.Text = "Sin sesión activa";
            ImgUsuarioMenu.Source = ImageSource.FromFile("user.png");
            LblRolUsuarioMenu.Text = "Invitado";
        }
    }

    public async Task AbrirCerrarMenu()
    {
        if (!isMenuOpen)
        {
            this.IsVisible = true;
            CargarUsuario();

            _ = OverlayFondo.FadeToAsync(1.0, 250);
            await PanelMenu.TranslateToAsync(0, 0, 300, Easing.CubicOut);
            isMenuOpen = true;
        }
        else
        {
            await CerrarMenuAnimado();
        }
    }

    private async Task CerrarMenuAnimado()
    {
        _ = OverlayFondo.FadeToAsync(0, 200);
        await PanelMenu.TranslateToAsync(340, 0, 250, Easing.CubicIn);
        isMenuOpen = false;
        this.IsVisible = false;
    }

    private async void OnCerrarMenuClicked(object? sender, EventArgs e)
    {
        await CerrarMenuAnimado();
    }

    private async void OnPerfilClicked(object? sender, EventArgs e)
    {
        await CerrarMenuAnimado();
        PerfilClicked?.Invoke(this, EventArgs.Empty);

        try
        {
            await Shell.Current.GoToAsync("Perfil");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al navegar a Perfil: {ex.Message}");
        }
    }

    private async void OnConfigurarClicked(object? sender, EventArgs e)
    {
        await CerrarMenuAnimado();
        ConfigurarClicked?.Invoke(this, EventArgs.Empty);

        try
        {
            await Shell.Current.GoToAsync("Configuracion");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al navegar a Configuración: {ex.Message}");
        }
    }

    private async void OnCerrarSesionClicked(object? sender, EventArgs e)
    {
        await CerrarMenuAnimado();

        Sesion.CerrarSesion();
        SesionCerrada?.Invoke(this, EventArgs.Empty);

        await Shell.Current.GoToAsync("//LoginPage");
    }
}

