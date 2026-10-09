namespace MovilL.views;

public partial class Navegador : ContentView
{
    public static readonly BindableProperty PaginaActivaProperty =
        BindableProperty.Create(
            nameof(PaginaActiva),
            typeof(string),
            typeof(Navegador),
            defaultValue: string.Empty,
            propertyChanged: OnPaginaActivaChanged);

    public string PaginaActiva
    {
        get => (string)GetValue(PaginaActivaProperty);
        set => SetValue(PaginaActivaProperty, value);
    }

    public event EventHandler? HomeClicked;
    public event EventHandler? MovimientoClicked;
    public event EventHandler? ResumenClicked;

    public Navegador()
    {
        InitializeComponent();
        ActualizarEstiloVisual();
    }

    private static void OnPaginaActivaChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Navegador navegador)
        {
            navegador.ActualizarEstiloVisual();
        }
    }

    private void ActualizarEstiloVisual()
    {
        var activa = PaginaActiva?.Trim() ?? string.Empty;

        // Home
        bool esHome = activa.Equals("Home", StringComparison.OrdinalIgnoreCase);
        TextHome.TextColor = esHome ? Color.FromArgb("#4CAF50") : Color.FromArgb("#8E95A5");
        TextHome.FontAttributes = esHome ? FontAttributes.Bold : FontAttributes.None;
        TabHome.Opacity = esHome ? 1.0 : 0.75;

        // Movimiento
        bool esMovimiento = activa.Equals("Movimiento", StringComparison.OrdinalIgnoreCase);
        TextMovimiento.TextColor = esMovimiento ? Color.FromArgb("#4CAF50") : Color.FromArgb("#8E95A5");
        TextMovimiento.FontAttributes = esMovimiento ? FontAttributes.Bold : FontAttributes.None;
        TabMovimiento.Opacity = esMovimiento ? 1.0 : 0.75;

        // Resumen
        bool esResumen = activa.Equals("Resumen", StringComparison.OrdinalIgnoreCase);
        TextResumen.TextColor = esResumen ? Color.FromArgb("#4CAF50") : Color.FromArgb("#8E95A5");
        TextResumen.FontAttributes = esResumen ? FontAttributes.Bold : FontAttributes.None;
        TabResumen.Opacity = esResumen ? 1.0 : 0.75;
    }

    private async void OnHomeTapped(object? sender, EventArgs e)
    {
        if (PaginaActiva?.Equals("Home", StringComparison.OrdinalIgnoreCase) == true)
            return;

        HomeClicked?.Invoke(this, EventArgs.Empty);

        try
        {
            await Shell.Current.GoToAsync("//Inicio");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al navegar a Inicio: {ex.Message}");
        }
    }

    private async void OnMovimientoTapped(object? sender, EventArgs e)
    {
        if (PaginaActiva?.Equals("Movimiento", StringComparison.OrdinalIgnoreCase) == true)
            return;

        MovimientoClicked?.Invoke(this, EventArgs.Empty);

        try
        {
            await Shell.Current.GoToAsync("//NuevoMovimientoPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al navegar a Movimiento: {ex.Message}");
        }
    }

    private async void OnResumenTapped(object? sender, EventArgs e)
    {
        if (PaginaActiva?.Equals("Resumen", StringComparison.OrdinalIgnoreCase) == true)
            return;

        ResumenClicked?.Invoke(this, EventArgs.Empty);

        try
        {
            await Shell.Current.GoToAsync("//ResumenPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al navegar a Resumen: {ex.Message}");
        }
    }
}

