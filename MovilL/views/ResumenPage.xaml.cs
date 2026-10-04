using MovilL.Models;

namespace MovilL.views;

public partial class ResumenPage : ContentPage
{
    public ResumenPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ActualizarResumen();
        PanelUsuario.CargarUsuario();
    }

    private void ActualizarResumen()
    {
        var ingresos = Sesion.Movimientos
            .Where(m => m.Tipo == "Ingreso")
            .Sum(m => m.Monto);

        var gastos = Sesion.Movimientos
            .Where(m => m.Tipo == "Gasto")
            .Sum(m => m.Monto);

        LabelIngresos.Text = ingresos.ToString("C");
        LabelGastos.Text = gastos.ToString("C");
        LabelBalance.Text = (ingresos - gastos).ToString("C");

        ListaMovimientos.ItemsSource = Sesion.Movimientos;
    }

    private async void OnUsuarioClicked(object? sender, EventArgs e)
    {
        await PanelUsuario.AbrirCerrarPanel();
    }
}