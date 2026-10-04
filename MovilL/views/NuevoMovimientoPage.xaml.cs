using MovilL.Models;

namespace MovilL.views;

public partial class NuevoMovimientoPage : ContentPage
{
    public NuevoMovimientoPage()
    {
        InitializeComponent();

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        PanelUsuario.CargarUsuario();
    }

    private async void OnUsuarioClicked(object? sender, EventArgs e)
    {
        await PanelUsuario.AbrirCerrarPanel();
    }

    private void OnGuardarClicked(object? sender, EventArgs e)
    {
        if (PickerTipo.SelectedItem == null || string.IsNullOrWhiteSpace(EntryCategoria.Text))
        {
            LabelMensaje.TextColor = Colors.Red;
            LabelMensaje.Text = "Completa tipo y categoría antes de guardar.";
            LabelMensaje.IsVisible = true;
            return;
        }

        var nuevo = new Movimiento
        {
            Tipo = PickerTipo.SelectedItem.ToString(),
            Categoria = EntryCategoria.Text,
            Monto = decimal.TryParse(EntryMonto.Text, out var monto) ? monto : 0,
            Fecha = (DateTime)DatePickerFecha.Date!
        };

        Sesion.Movimientos.Add(nuevo);

        LabelMensaje.TextColor = Colors.Green;
        LabelMensaje.Text = Sesion.EsInvitado
            ? "Guardado (como invitado, no se conservará al salir)."
            : "Movimiento guardado correctamente.";
        LabelMensaje.IsVisible = true;

        // Limpiar formulario
        PickerTipo.SelectedItem = null;
        EntryCategoria.Text = string.Empty;
        EntryMonto.Text = string.Empty;
        DatePickerFecha.Date = DateTime.Today;
    }
}