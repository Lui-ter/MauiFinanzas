using Microsoft.Maui.Graphics;
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
        MenuUsuario.CargarUsuario();
        ToolbarUsuario.IconImageSource = Sesion.UsuarioActual?.Imagen ?? "user.png";
    }

    private void ActualizarResumen()
    {
        // 1. Calcular Totales desde la Sesion (insensible a mayusculas/minusculas)
        var ingresos = Sesion.Movimientos
            .Where(m => m.Tipo.Equals("Ingreso", StringComparison.OrdinalIgnoreCase))
            .Sum(m => m.Monto);

        var gastos = Sesion.Movimientos
            .Where(m => m.Tipo.Equals("Gasto", StringComparison.OrdinalIgnoreCase))
            .Sum(m => m.Monto);

        var balance = ingresos - gastos;

        // 2. Actualizar Labels
        LabelIngresos.Text = ingresos.ToString("C");
        LabelGastos.Text = gastos.ToString("C");
        LabelBalance.Text = balance.ToString("C");

        // 3. Refrescar Lista de Movimientos (Revertido para ver los más recientes primero)
        ListaMovimientos.ItemsSource = null;
        ListaMovimientos.ItemsSource = Enumerable.Reverse(Sesion.Movimientos).ToList();

        // 4. Redibujar la gráfica con los valores reales
        GraficaDonaView.Drawable = new DonaDrawable((float)ingresos, (float)gastos);
        GraficaDonaView.Invalidate();
    }

    private async void OnUsuarioClicked(object? sender, EventArgs e)
    {
        await MenuUsuario.AbrirCerrarMenu();
    }
}

// Dibujante nativo para el GraphicsView: Verde = Ingresos / Rojo = Gastos
public class DonaDrawable : IDrawable
{
    private readonly float _ingresos;
    private readonly float _gastos;

    public DonaDrawable(float ingresos, float gastos)
    {
        _ingresos = Math.Max(0, ingresos);
        _gastos = Math.Max(0, gastos);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float total = _ingresos + _gastos;

        float cx = dirtyRect.Width / 2;
        float cy = dirtyRect.Height / 2;
        float radius = Math.Min(cx, cy) - 10;

        canvas.StrokeSize = 13;
        canvas.Antialias = true;

        // 1. Si no hay datos registrados: anillo neutro gris
        if (total <= 0)
        {
            canvas.StrokeColor = Color.FromArgb("#2D3042");
            canvas.DrawCircle(cx, cy, radius);
            return;
        }

        // 2. Si solo hay ingresos: 100% Verde
        if (_gastos <= 0)
        {
            canvas.StrokeColor = Color.FromArgb("#81C784");
            canvas.DrawCircle(cx, cy, radius);
            return;
        }

        // 3. Si solo hay gastos: 100% Rojo
        if (_ingresos <= 0)
        {
            canvas.StrokeColor = Color.FromArgb("#E57373");
            canvas.DrawCircle(cx, cy, radius);
            return;
        }

        // 4. Ambos presentes:
        // Dibujamos la base completa en ROJO (Gastos) con DrawCircle
        canvas.StrokeColor = Color.FromArgb("#E57373");
        canvas.DrawCircle(cx, cy, radius);

        // Y encima dibujamos la porción proporcional exacta de INGRESOS en VERDE
        float proporcionIngresos = _ingresos / total;
        float anguloIngresos = Math.Clamp(proporcionIngresos * 360f, 1f, 359f);

        canvas.StrokeColor = Color.FromArgb("#81C784");
        // Dibuja en sentido horario empezando a las 12:00 (-90°)
        canvas.DrawArc(cx - radius, cy - radius, radius * 2, radius * 2, -90, -90 + anguloIngresos, true, false);
    }
}