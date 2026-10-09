using MovilL.Models;

namespace MovilL.views;

public partial class NuevoMovimientoPage : ContentPage
{
    public NuevoMovimientoPage()
    {
        InitializeComponent();

        // Configuración inicial de la vista
        DatePickerFecha.Date = DateTime.Today;
        PickerTipo.SelectedIndex = 0; // Inicia seleccionado en "Ingreso" por defecto
        CargarCategorias("Ingreso");
        ActualizarPreview();
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

    // Se dispara cuando se cambia entre "Ingreso" y "Gasto"
    private void OnTipoChanged(object? sender, EventArgs e)
    {
        string tipo = PickerTipo.SelectedItem?.ToString() ?? "Ingreso";
        CargarCategorias(tipo);
        ActualizarPreview();
    }

    // Carga las categorías del modelo en memoria filtradas por tipo (Ingreso / Gasto)
    private void CargarCategorias(string tipo)
    {
        var categoriasFiltradas = Sesion.Categorias
            .Where(c => c.Tipo.Equals(tipo, StringComparison.OrdinalIgnoreCase))
            .ToList();

        PickerCategoria.ItemsSource = categoriasFiltradas;

        if (categoriasFiltradas.Count > 0)
        {
            PickerCategoria.SelectedIndex = 0;
        }
    }

    // Se dispara cuando cambia cualquier input en el formulario (monto, fecha, categoría seleccionada)
    private void OnFormularioChanged(object? sender, EventArgs e)
    {
        ActualizarPreview();
    }

    // Lógica dinámica de la vista previa y botón de guardado
    private void ActualizarPreview()
    {
        // 1. Tipo (Ingreso / Gasto)
        string tipo = PickerTipo.SelectedItem?.ToString() ?? "Ingreso";
        bool esIngreso = tipo.Equals("Ingreso", StringComparison.OrdinalIgnoreCase);
        Color colorTema = esIngreso ? Color.FromArgb("#4CAF50") : Color.FromArgb("#FF5252");

        LabelPreviewTipo.Text = tipo.ToUpper();
        LabelPreviewTipo.TextColor = colorTema;

        // Botón de guardar dinámico según tipo
        BtnGuardar.Text = esIngreso ? "Guardar ingreso" : "Guardar gasto";
        BtnGuardar.BackgroundColor = colorTema;

        // Borde del cuadro de la imagen adaptado al color del tipo
        BorderIcono.Stroke = colorTema;

        // 2. Carga de la imagen y nombre desde el modelo Categoria (sin icono emoji)
        var categoriaSeleccionada = PickerCategoria.SelectedItem as Categoria;

        if (categoriaSeleccionada != null)
        {
            LabelPreviewCategoria.Text = categoriaSeleccionada.Nombre;

            if (!string.IsNullOrWhiteSpace(categoriaSeleccionada.Imagen))
            {
                ImagePreviewCategoria.Source = ImageSource.FromFile(categoriaSeleccionada.Imagen);
                ImagePreviewCategoria.IsVisible = true;
            }
            else
            {
                ImagePreviewCategoria.Source = null;
                ImagePreviewCategoria.IsVisible = false;
            }
        }
        else
        {
            LabelPreviewCategoria.Text = "Sin categoría";
            ImagePreviewCategoria.Source = null;
            ImagePreviewCategoria.IsVisible = false;
        }

        // 3. Monto dinámico
        if (decimal.TryParse(EntryMonto.Text, out var monto) && monto > 0)
        {
            string signo = esIngreso ? "+" : "-";
            LabelPreviewMonto.Text = $"{signo}$ {monto:N2}";
            LabelPreviewMonto.TextColor = colorTema;
        }
        else
        {
            LabelPreviewMonto.Text = "$ 0,00";
            LabelPreviewMonto.TextColor = Colors.White;
        }

        // 4. Fecha dinámica
        DateTime fecha = DatePickerFecha.Date ?? DateTime.Today;
        LabelPreviewFecha.Text = fecha.ToString("dd/MM/yyyy");
    }

    // Lógica del botón de guardado alojado en la Preview
    private void OnGuardarClicked(object? sender, EventArgs e)
    {
        var categoriaSeleccionada = PickerCategoria.SelectedItem as Categoria;

        if (PickerTipo.SelectedItem == null || categoriaSeleccionada == null)
        {
            LabelMensaje.TextColor = Colors.Red;
            LabelMensaje.Text = "Completa tipo y categoría antes de guardar.";
            LabelMensaje.IsVisible = true;
            return;
        }

        var nuevo = new Movimiento
        {
            Tipo = PickerTipo.SelectedItem.ToString() ?? "Ingreso",
            Categoria = categoriaSeleccionada.Nombre,
            Monto = decimal.TryParse(EntryMonto.Text, out var monto) ? monto : 0,
            Fecha = DatePickerFecha.Date ?? DateTime.Today
        };

        Sesion.Movimientos.Add(nuevo);

        LabelMensaje.TextColor = Colors.Green;
        LabelMensaje.Text = Sesion.EsInvitado
            ? "Guardado (como invitado, no se conservará al salir)."
            : "Movimiento guardado correctamente.";
        LabelMensaje.IsVisible = true;

        // Limpiar formulario y restablecer la preview
        EntryMonto.Text = string.Empty;
        DatePickerFecha.Date = DateTime.Today;
        PickerTipo.SelectedIndex = 0;
        CargarCategorias("Ingreso");

        ActualizarPreview();
    }
}

