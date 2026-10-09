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
        // 1. Calcular Totales desde la Sesion
        var ingresos = Sesion.Movimientos
            .Where(m => m.Tipo == "Ingreso")
            .Sum(m => m.Monto);

        var gastos = Sesion.Movimientos
            .Where(m => m.Tipo == "Gasto")
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

// Dibujante nativo para el GraphicsView
public class DonaDrawable : IDrawable
{
    private readonly float _ingresos;
    private readonly float _gastos;

    public DonaDrawable(float ingresos, float gastos)
    {
        _ingresos = ingresos;
        _gastos = gastos;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float total = _ingresos + _gastos;

        // Si no hay datos registrados, dibuja un anillo gris por defecto
        if (total <= 0)
        {
            float cxGris = dirtyRect.Width / 2;
            float cyGris = dirtyRect.Height / 2;
            float radiusGris = Math.Min(cxGris, cyGris) - 10;

            canvas.StrokeSize = 12;
            canvas.StrokeColor = Color.FromArgb("#2D3042");
            canvas.DrawArc(cxGris - radiusGris, cyGris - radiusGris, radiusGris * 2, radiusGris * 2, 0, 360, true, false);
            return;
        }

        // Cálculo de ángulos proporcionales
        float anguloIngresos = (_ingresos / total) * 360;
        float anguloGastos = (_gastos / total) * 360;

        float cx = dirtyRect.Width / 2;
        float cy = dirtyRect.Height / 2;
        float radius = Math.Min(cx, cy) - 10;

        canvas.StrokeSize = 12;
        canvas.Antialias = true;

        // Dibujar sección de Ingresos (Verde)
        if (_ingresos > 0)
        {
            canvas.StrokeColor = Color.FromArgb("#81C784");
            canvas.DrawArc(cx - radius, cy - radius, radius * 2, radius * 2, -90, -90 + anguloIngresos, true, false);
        }

        // Dibujar sección de Gastos (Rojo)
        if (_gastos > 0)
        {
            canvas.StrokeColor = Color.FromArgb("#E57373");
            canvas.DrawArc(cx - radius, cy - radius, radius * 2, radius * 2, -90 + anguloIngresos, -90 + anguloIngresos + anguloGastos, true, false);
        }
    }
}