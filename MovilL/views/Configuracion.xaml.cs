using MovilL.Models;

namespace MovilL.views;

public partial class Configuracion : ContentPage
{
    public Configuracion()
    {
        InitializeComponent();
        PickerMoneda.SelectedIndex = 0; // COP por defecto
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MenuUsuario.CargarUsuario();
        ToolbarUsuario.IconImageSource = Sesion.UsuarioActual?.Imagen ?? "user.png";
    }

    private async void OnUsuarioClicked(object? sender, EventArgs e)
    {
        await MenuUsuario.AbrirCerrarMenu();
    }

    private async void OnGuardarAjustesClicked(object? sender, EventArgs e)
    {
        await DisplayAlertAsync("Configuración", "Los ajustes han sido guardados exitosamente.", "Aceptar");
        await Shell.Current.GoToAsync("..");
    }
}

