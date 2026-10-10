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

    // READ: Carga los datos del modelo actual en la interfaz
    private void CargarDatosPerfil()
    {
        var usuario = Sesion.UsuarioActual;

        if (usuario != null)
        {
            LblNombreHeader.Text = usuario.Nombre;
            LblUsernameHeader.Text = "@" + (string.IsNullOrEmpty(usuario.NombreUsuario) ? "usuario" : usuario.NombreUsuario);
            string imagen = string.IsNullOrWhiteSpace(usuario.Imagen) ? "user.png" : usuario.Imagen;
            ImgPerfil.Source = imagen;

            EntryNombre.Text = usuario.Nombre;
            EntryUsername.Text = usuario.NombreUsuario;
            EntryCorreo.Text = usuario.Correo;
            EntryEdad.Text = usuario.Edad > 0 ? usuario.Edad.ToString() : "";
            EntryContrasena.Text = usuario.Contrasena;

            bool esInvitado = Sesion.EsInvitado || usuario.Id == 0;
            LblTipoCuenta.Text = esInvitado ? "Invitado" : "Principal";
            LblTipoCuenta.TextColor = esInvitado ? Color.FromArgb("#FFCA28") : Color.FromArgb("#81C784");
            BtnRegistrarPermanente.IsVisible = esInvitado;

            LblTotalMovimientos.Text = Sesion.Movimientos.Count.ToString();
        }
        else
        {
            LblNombreHeader.Text = "Invitado";
            LblUsernameHeader.Text = "@invitado";
            ImgPerfil.Source = "user.png";
            EntryNombre.Text = "Invitado";
            EntryUsername.Text = "invitado";
            EntryCorreo.Text = "invitado@movill.com";
            EntryEdad.Text = "";
            EntryContrasena.Text = "";
            LblTipoCuenta.Text = "Invitado";
            LblTipoCuenta.TextColor = Color.FromArgb("#FFCA28");
            BtnRegistrarPermanente.IsVisible = true;
            LblTotalMovimientos.Text = Sesion.Movimientos.Count.ToString();
        }

        LabelMensajePerfil.IsVisible = false;
    }

    private async void OnUsuarioClicked(object? sender, EventArgs e)
    {
        await MenuUsuario.AbrirCerrarMenu();
    }

    // Alternar visibilidad de contraseña
    private void OnToggleContrasenaClicked(object? sender, EventArgs e)
    {
        EntryContrasena.IsPassword = !EntryContrasena.IsPassword;
        BtnToggleContrasena.Text = EntryContrasena.IsPassword ? "👁 Mostrar" : "🔒 Ocultar";
    }

    // UPDATE: Modifica los datos del usuario y los sincroniza con el modelo
    private async void OnGuardarCambiosClicked(object? sender, EventArgs e)
    {
        string nombre = EntryNombre.Text?.Trim() ?? string.Empty;
        string username = EntryUsername.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        string correo = EntryCorreo.Text?.Trim() ?? string.Empty;
        string contrasena = EntryContrasena.Text?.Trim() ?? string.Empty;

        // Validaciones
        if (string.IsNullOrWhiteSpace(nombre))
        {
            await MostrarMensaje("El nombre completo no puede estar vacío.", false);
            return;
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            await MostrarMensaje("El nombre de usuario no puede estar vacío.", false);
            return;
        }

        int idActual = Sesion.UsuarioActual?.Id ?? 0;
        if (Usuario.ExisteNombreUsuario(username, idActual))
        {
            await MostrarMensaje($"El usuario '@{username}' ya está en uso. Elige otro.", false);
            return;
        }

        int edad = 0;
        if (!string.IsNullOrWhiteSpace(EntryEdad.Text) && (!int.TryParse(EntryEdad.Text.Trim(), out edad) || edad < 0))
        {
            await MostrarMensaje("Por favor ingresa una edad numérica válida.", false);
            return;
        }

        if (Sesion.UsuarioActual != null)
        {
            // 1. Actualizar instancia de sesión
            Sesion.UsuarioActual.Nombre = nombre;
            Sesion.UsuarioActual.NombreUsuario = username;
            Sesion.UsuarioActual.Correo = correo;
            Sesion.UsuarioActual.Edad = edad;
            Sesion.UsuarioActual.Contrasena = contrasena;

            // 2. Reflejar en el modelo central Usuario.ListaUsuarios
            Usuario.ActualizarUsuario(Sesion.UsuarioActual);

            Sesion.NombreUsuario = nombre;

            // 3. Actualizar vistas
            LblNombreHeader.Text = nombre;
            LblUsernameHeader.Text = "@" + username;
            MenuUsuario.CargarUsuario();

            await MostrarMensaje("¡Datos modificados y guardados en el modelo correctamente!", true);
        }
    }

    // CREATE: Convertir cuenta de invitado en usuario permanente en el modelo
    private async void OnRegistrarPermanenteClicked(object? sender, EventArgs e)
    {
        string nombre = EntryNombre.Text?.Trim() ?? string.Empty;
        string username = EntryUsername.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        string correo = EntryCorreo.Text?.Trim() ?? string.Empty;
        string contrasena = EntryContrasena.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(contrasena))
        {
            await MostrarMensaje("Completa nombre, usuario y contraseña para registrar la cuenta.", false);
            return;
        }

        if (Usuario.ExisteNombreUsuario(username))
        {
            await MostrarMensaje($"El usuario '@{username}' ya está registrado. Elige otro.", false);
            return;
        }

        int.TryParse(EntryEdad.Text?.Trim(), out int edad);

        var nuevoUsuario = new Usuario
        {
            Nombre = nombre,
            NombreUsuario = username,
            Correo = correo,
            Contrasena = contrasena,
            Edad = edad,
            Imagen = "user.png"
        };

        // Guardar en la lista del modelo
        Usuario.ActualizarUsuario(nuevoUsuario);

        Sesion.EsInvitado = false;
        Sesion.EstaLogueado = true;
        Sesion.UsuarioActual = nuevoUsuario;
        Sesion.NombreUsuario = nuevoUsuario.Nombre;

        CargarDatosPerfil();
        MenuUsuario.CargarUsuario();

        await DisplayAlertAsync("Registro Exitoso", $"Tu cuenta '@{username}' ha sido creada y guardada en el modelo.", "Aceptar");
    }

    // READ / REVERT: Descartar cambios sin guardar y restablecer los datos del modelo
    private void OnRestablecerValoresClicked(object? sender, EventArgs e)
    {
        CargarDatosPerfil();
        _ = MostrarMensaje("Valores restablecidos según el modelo actual.", true);
    }

    // DELETE: Elimina el usuario del modelo y finaliza la sesión
    private async void OnEliminarCuentaClicked(object? sender, EventArgs e)
    {
        bool confirmar = await DisplayAlertAsync("Eliminar Cuenta",
            "¿Estás seguro de que deseas eliminar tu usuario y datos? Esta operación no se puede deshacer.",
            "Eliminar", "Cancelar");

        if (confirmar)
        {
            if (Sesion.UsuarioActual != null && Sesion.UsuarioActual.Id > 0)
            {
                Usuario.EliminarUsuario(Sesion.UsuarioActual.Id);
            }

            Sesion.CerrarSesion();
            await DisplayAlertAsync("Cuenta Eliminada", "El usuario ha sido eliminado del modelo y se cerró la sesión.", "Aceptar");
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }

    private async Task MostrarMensaje(string mensaje, bool esExito)
    {
        LabelMensajePerfil.Text = mensaje;
        LabelMensajePerfil.TextColor = esExito ? Color.FromArgb("#81C784") : Color.FromArgb("#FF5252");
        LabelMensajePerfil.IsVisible = true;

        if (esExito)
        {
            await DisplayAlertAsync("Perfil", mensaje, "Aceptar");
        }
    }
}

