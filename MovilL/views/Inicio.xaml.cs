using MovilL.Models;
using MovilL.Services;

namespace MovilL.views;

public partial class Inicio : ContentPage
{
    private readonly IMovilDatabaseService? _databaseService;

    public Inicio(IMovilDatabaseService databaseService)
    {
        _databaseService = databaseService;
        InitializeComponent();
    }

    public Inicio()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_databaseService != null)
        {
            try
            {
                var movimientos = await _databaseService.GetMovimientosAsync(Sesion.UsuarioActual?.Id ?? 0);
                Sesion.Movimientos.Clear();
                foreach (var movimiento in movimientos)
                    Sesion.Movimientos.Add(movimiento);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"No se pudieron cargar los movimientos: {ex.Message}");
            }
        }
        ActualizarDashboard();
        MenuUsuario.CargarUsuario();
        ToolbarUsuario.IconImageSource = Sesion.UsuarioActual?.Imagen ?? "user.png";
    }

    private void ActualizarDashboard()
    {
        // Saludo personalizado
        if (Sesion.EsInvitado)
        {
            LabelSaludo.Text = "¡Hola, Invitado!";
        }
        else if (!string.IsNullOrWhiteSpace(Sesion.NombreUsuario))
        {
            LabelSaludo.Text = $"¡Hola, {Sesion.NombreUsuario}!";
        }
        else
        {
            LabelSaludo.Text = "¡Bienvenido a MovilL!";
        }

        // Totales rápidos en memoria
        var ingresos = Sesion.Movimientos
            .Where(m => m.Tipo == "Ingreso")
            .Sum(m => m.Monto);

        var gastos = Sesion.Movimientos
            .Where(m => m.Tipo == "Gasto")
            .Sum(m => m.Monto);

        var balance = ingresos - gastos;

        LabelBalanceRapido.Text = balance.ToString("C");
        LabelIngresosRapido.Text = ingresos.ToString("C");
        LabelGastosRapido.Text = gastos.ToString("C");
    }

    private async void OnUsuarioClicked(object? sender, EventArgs e)
    {
        await MenuUsuario.AbrirCerrarMenu();
    }

    private async void OnIrAMovimientoClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//NuevoMovimientoPage");
    }

    private async void OnIrAResumenClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ResumenPage");
    }
}

