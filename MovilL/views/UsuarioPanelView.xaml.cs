using MovilL.Models;

namespace MovilL.Views.Controls;

public partial class UsuarioPanelView : ContentView
{
    private bool isPanelOpen = false;

    // Eventos para que la página "padre" reaccione si lo necesita
    public event EventHandler? PerfilClicked;
    public event EventHandler? ConfigurarClicked;
    public event EventHandler? SesionCerrada;

    public UsuarioPanelView()
    {
        InitializeComponent();
        CargarUsuario();
    }

    public void CargarUsuario()
    {
        LblNombreUsuario.Text = Sesion.EsInvitado
            ? "Invitado"
            : Sesion.NombreUsuario;

        ImgUsuario.Source = ImageSource.FromFile("user.png");
    }

    // Método público para que cualquier página abra/cierre el panel
    public async Task AbrirCerrarPanel()
    {
        if (!isPanelOpen)
        {
            await UsuarioPanel.TranslateTo(0, 0, 300, Easing.CubicOut);
            isPanelOpen = true;
        }
        else
        {
            await UsuarioPanel.TranslateTo(300, 0, 300, Easing.CubicIn);
            isPanelOpen = false;
        }
    }

    private void OnPerfilClicked(object? sender, EventArgs e)
    {
        PerfilClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnConfigurarClicked(object? sender, EventArgs e)
    {
        ConfigurarClicked?.Invoke(this, EventArgs.Empty);
    }

    private async void OnCerrarSesionClicked(object? sender, EventArgs e)
    {
        Sesion.CerrarSesion();

        SesionCerrada?.Invoke(this, EventArgs.Empty);

        await Shell.Current.GoToAsync("//LoginPage");
    }
}