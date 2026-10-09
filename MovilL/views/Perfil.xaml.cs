using MovilL.Models;

namespace MovilL.views;

public partial class Perfil : ContentPage
{
    public Perfil()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarDatosPerfil();
        MenuUsuario.CargarUsuario();
        ToolbarUsuario.IconImageSource = Sesion.UsuarioActual?.Imagen ?? "user.png";
    }

    private void CargarDatosPerfil()
    {
        var usuario = Sesion.UsuarioActual;

        if (usuario != null)
        {
            LblNombreHeader.Text = usuario.Nombre;
            LblUsernameHeader.Text = "@" + (string.IsNullOrEmpty(usuario.NombreUsuario) ? "usuario" : usuario.NombreUsuario);
            string imagen = string.IsNullOrWhiteSpace(usuario.Imagen) ? "user.png" : usuario.Imagen;
            ImgPerfil.Source = ImageSource.FromFile(imagen);

            EntryNombre.Text = usuario.Nombre;
            EntryCorreo.Text = usuario.Correo;
            EntryEdad.Text = usuario.Edad > 0 ? usuario.Edad.ToString() : "";

            LblTipoCuenta.Text = Sesion.EsInvitado ? "Invitado" : "Principal";
            LblTotalMovimientos.Text = Sesion.Movimientos.Count.ToString();
        }
        else
        {
            LblNombreHeader.Text = "Invitado";
            LblUsernameHeader.Text = "@invitado";
            ImgPerfil.Source = ImageSource.FromFile("user.png");
            EntryNombre.Text = "Invitado";
            EntryCorreo.Text = "invitado@movill.com";
            EntryEdad.Text = "0";
            LblTipoCuenta.Text = "Invitado";
            LblTotalMovimientos.Text = Sesion.Movimientos.Count.ToString();
        }
    }

    private async void OnUsuarioClicked(object? sender, EventArgs e)
    {
        await MenuUsuario.AbrirCerrarMenu();
    }

    private async void OnGuardarCambiosClicked(object? sender, EventArgs e)
    {
        if (Sesion.UsuarioActual != null)
        {
            Sesion.UsuarioActual.Nombre = EntryNombre.Text?.Trim() ?? Sesion.UsuarioActual.Nombre;
            Sesion.UsuarioActual.Correo = EntryCorreo.Text?.Trim() ?? Sesion.UsuarioActual.Correo;

            if (int.TryParse(EntryEdad.Text?.Trim(), out int edad))
            {
                Sesion.UsuarioActual.Edad = edad;
            }

            Sesion.NombreUsuario = Sesion.UsuarioActual.Nombre;
            LblNombreHeader.Text = Sesion.UsuarioActual.Nombre;
            MenuUsuario.CargarUsuario();

            await DisplayAlertAsync("Perfil", "Datos actualizados correctamente.", "Aceptar");
        }
        else
        {
            await DisplayAlertAsync("Perfil", "No es posible guardar cambios en modo invitado.", "Aceptar");
        }
    }
}

