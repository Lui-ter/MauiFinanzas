using MovilL.Models;
using MovilL.Services;

namespace MovilL.views;

public partial class NuevoMovimientoPage : ContentPage
{
    private readonly IMovilDatabaseService _databaseService;
    private bool _cargandoCategorias;
    private byte[]? _imagenCategoriaSeleccionada;
    private string _extensionImagenCategoria = ".png";

    public NuevoMovimientoPage(IMovilDatabaseService databaseService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        InitializeComponent();
        Initialize();
    }

    private void Initialize()
    {
        DatePickerFecha.Date = DateTime.Today;
        PickerTipo.SelectedIndex = 0;
        PickerTipoCategoria.SelectedIndex = 0;
        CargarCategorias("Ingreso");
        ActualizarPreview();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        MenuUsuario.CargarUsuario();
        ToolbarUsuario.IconImageSource = Sesion.UsuarioActual?.Imagen ?? "user.png";
        await CargarDatosAsync();
    }

    private async Task CargarDatosAsync()
    {
        try
        {
            int usuarioId = Sesion.UsuarioActual?.Id ?? 0;
            Sesion.Categorias = Categoria.ListaCategorias;

            var categorias = await _databaseService.GetCategoriasAsync(usuarioId);
            if (usuarioId > 0)
            {
                var movimientos = await _databaseService.GetMovimientosAsync(usuarioId);
                Sesion.Movimientos.Clear();
                Sesion.Movimientos.AddRange(movimientos);
            }

            Sesion.Categorias = categorias;
            _cargandoCategorias = true;
            try
            {
                CargarCategorias(PickerTipo.SelectedItem?.ToString() ?? "Ingreso");
            }
            finally
            {
                _cargandoCategorias = false;
            }

            ActualizarPreview();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudieron cargar los datos de movimientos: {ex}");
            await DisplayAlertAsync("Error", "No se pudieron cargar tus categorías o movimientos. Inténtalo de nuevo.", "Aceptar");
        }
    }

    private async void OnUsuarioClicked(object? sender, EventArgs e)
    {
        await MenuUsuario.AbrirCerrarMenu();
    }

    private void OnTipoChanged(object? sender, EventArgs e)
    {
        string tipo = PickerTipo.SelectedItem?.ToString() ?? "Ingreso";
        _cargandoCategorias = true;
        try
        {
            CargarCategorias(tipo);
        }
        finally
        {
            _cargandoCategorias = false;
        }
        ActualizarPreview();
    }

    private void CargarCategorias(string tipo)
    {
        var categoriasFiltradas = Sesion.Categorias
            .Where(c => c.Tipo.Equals(tipo, StringComparison.OrdinalIgnoreCase))
            .ToList();

        PickerCategoria.ItemsSource = categoriasFiltradas;
        PickerCategoria.SelectedIndex = categoriasFiltradas.Count > 0 ? 0 : -1;
    }

    private void OnFormularioChanged(object? sender, EventArgs e)
    {
        if (_cargandoCategorias)
            return;

        ActualizarPreview();
    }

    private void ActualizarPreview()
    {
        string tipo = PickerTipo.SelectedItem?.ToString() ?? "Ingreso";
        bool esIngreso = tipo.Equals("Ingreso", StringComparison.OrdinalIgnoreCase);
        Color colorTema = esIngreso ? Color.FromArgb("#49CE8D") : Color.FromArgb("#FF8585");

        LabelPreviewTipo.Text = tipo.ToUpper();
        LabelPreviewTipo.TextColor = colorTema;
        BtnGuardar.Text = esIngreso ? "Guardar ingreso" : "Guardar gasto";
        BtnGuardar.BackgroundColor = colorTema;
        BtnGuardar.TextColor = esIngreso ? Color.FromArgb("#0B2018") : Color.FromArgb("#301111");
        BorderIcono.Stroke = colorTema;

        var categoriaSeleccionada = PickerCategoria.SelectedItem as Categoria;
        if (categoriaSeleccionada != null)
        {
            LabelPreviewCategoria.Text = categoriaSeleccionada.Nombre;
            if (!string.IsNullOrWhiteSpace(categoriaSeleccionada.Imagen))
            {
                ImagePreviewCategoria.Source = categoriaSeleccionada.Imagen;
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

        DateTime fecha = DatePickerFecha.Date ?? DateTime.Today;
        LabelPreviewFecha.Text = fecha.ToString("dd/MM/yyyy");
    }

    private void OnMostrarFormularioCategoriaClicked(object? sender, EventArgs e)
    {
        FormularioCategoria.IsVisible = true;
        int usuarioId = Sesion.UsuarioActual?.Id ?? 0;
        BtnGuardarCategoria.IsEnabled = usuarioId > 0;

        if (usuarioId <= 0)
        {
            MostrarMensajeCategoria(
                "Para guardar categorías personales, inicia sesión con una cuenta.",
                Color.FromArgb("#FF8585"));
        }
        else
        {
            LabelCategoriaMensaje.IsVisible = false;
            EntryNuevaCategoria.Focus();
        }
    }

    private void OnOcultarFormularioCategoriaClicked(object? sender, EventArgs e)
    {
        FormularioCategoria.IsVisible = false;
        LabelCategoriaMensaje.IsVisible = false;
        EntryNuevaCategoria.Text = string.Empty;
        PickerTipoCategoria.SelectedIndex = 0;
        LimpiarImagenCategoria();
    }

    private async void OnSeleccionarImagenCategoriaClicked(object? sender, EventArgs e)
    {
        try
        {
            var imagen = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Selecciona una imagen para la categoría",
                FileTypes = FilePickerFileType.Images
            });

            if (imagen == null)
                return;

            string extension = Path.GetExtension(imagen.FileName);
            if (!extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
            {
                MostrarMensajeCategoria("Selecciona una imagen JPG o PNG.", Color.FromArgb("#FF8585"));
                return;
            }

            await using var stream = await imagen.OpenReadAsync();
            using var memory = new MemoryStream();
            var buffer = new byte[81920];
            int bytesRead;
            const int maximoBytes = 10 * 1024 * 1024;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                if (memory.Length + bytesRead > maximoBytes)
                {
                    MostrarMensajeCategoria("La imagen no puede superar los 10 MB.", Color.FromArgb("#FF8585"));
                    return;
                }

                await memory.WriteAsync(buffer, 0, bytesRead);
            }

            byte[] imagenPreview = memory.ToArray();
            _imagenCategoriaSeleccionada = imagenPreview;
            _extensionImagenCategoria = extension.ToLowerInvariant();
            ImagePreviewNuevaCategoria.Source = ImageSource.FromStream(() => new MemoryStream(imagenPreview));
            ImagePreviewNuevaCategoria.IsVisible = true;
            LabelSinImagenCategoria.IsVisible = false;
            LabelCategoriaMensaje.IsVisible = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudo seleccionar la imagen de categoría: {ex}");
            MostrarMensajeCategoria("No se pudo cargar la imagen seleccionada.", Color.FromArgb("#FF8585"));
        }
    }

    private async void OnGuardarCategoriaClicked(object? sender, EventArgs e)
    {
        int usuarioId = Sesion.UsuarioActual?.Id ?? 0;
        if (usuarioId <= 0)
        {
            MostrarMensajeCategoria(
                "Para guardar categorías personales, inicia sesión con una cuenta.",
                Color.FromArgb("#FF8585"));
            return;
        }

        string nombre = EntryNuevaCategoria.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nombre))
        {
            MostrarMensajeCategoria("Escribe un nombre para la categoría.", Color.FromArgb("#FF8585"));
            return;
        }

        string tipo = PickerTipoCategoria.SelectedItem?.ToString() ?? "Ingreso";
        string? rutaImagen = null;
        Categoria categoria;
        try
        {
            if (await _databaseService.ExisteCategoriaAsync(nombre, tipo, usuarioId))
            {
                MostrarMensajeCategoria("Ya existe una categoría con ese nombre para este tipo.", Color.FromArgb("#FF8585"));
                return;
            }

            string imagen = string.Empty;
            if (_imagenCategoriaSeleccionada != null)
            {
                string carpetaImagenes = Path.Combine(FileSystem.AppDataDirectory, "categorias");
                Directory.CreateDirectory(carpetaImagenes);
                rutaImagen = Path.Combine(carpetaImagenes, $"categoria-{Guid.NewGuid():N}{_extensionImagenCategoria}");
                await File.WriteAllBytesAsync(rutaImagen, _imagenCategoriaSeleccionada);
                imagen = rutaImagen;
            }

            categoria = new Categoria
            {
                UsuarioId = usuarioId,
                Nombre = nombre,
                Tipo = tipo,
                Imagen = imagen
            };

            await _databaseService.GuardarCategoriaAsync(categoria);
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrEmpty(rutaImagen) && File.Exists(rutaImagen))
            {
                try
                {
                    File.Delete(rutaImagen);
                }
                catch (IOException cleanupException)
                {
                    System.Diagnostics.Debug.WriteLine($"No se pudo limpiar la imagen temporal: {cleanupException}");
                }
                catch (UnauthorizedAccessException cleanupException)
                {
                    System.Diagnostics.Debug.WriteLine($"No se pudo limpiar la imagen temporal: {cleanupException}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"No se pudo guardar la categoría: {ex}");
            MostrarMensajeCategoria("No se pudo guardar la categoría. Inténtalo de nuevo.", Color.FromArgb("#FF8585"));
            return;
        }

        Sesion.Categorias = Sesion.Categorias.Append(categoria).ToList();
        if (tipo.Equals(PickerTipo.SelectedItem?.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            _cargandoCategorias = true;
            try
            {
                CargarCategorias(tipo);
                PickerCategoria.SelectedItem = categoria;
            }
            finally
            {
                _cargandoCategorias = false;
            }

            ActualizarPreview();
        }

        EntryNuevaCategoria.Text = string.Empty;
        LimpiarImagenCategoria();
        MostrarMensajeCategoria("Categoría guardada solo en tu cuenta.", Color.FromArgb("#68E0A3"));
    }

    private void LimpiarImagenCategoria()
    {
        _imagenCategoriaSeleccionada = null;
        _extensionImagenCategoria = ".png";
        ImagePreviewNuevaCategoria.Source = null;
        ImagePreviewNuevaCategoria.IsVisible = false;
        LabelSinImagenCategoria.IsVisible = true;
    }

    private void MostrarMensajeCategoria(string mensaje, Color color)
    {
        LabelCategoriaMensaje.Text = mensaje;
        LabelCategoriaMensaje.TextColor = color;
        LabelCategoriaMensaje.IsVisible = true;
    }

    private async void OnGuardarClicked(object? sender, EventArgs e)
    {
        var categoriaSeleccionada = PickerCategoria.SelectedItem as Categoria;
        if (PickerTipo.SelectedItem == null || categoriaSeleccionada == null)
        {
            LabelMensaje.TextColor = Color.FromArgb("#FF8585");
            LabelMensaje.Text = "Selecciona un tipo y una categoría antes de guardar.";
            LabelMensaje.IsVisible = true;
            return;
        }

        if (!decimal.TryParse(EntryMonto.Text, out var monto) || monto <= 0)
        {
            LabelMensaje.TextColor = Color.FromArgb("#FF8585");
            LabelMensaje.Text = "El monto debe ser mayor que cero.";
            LabelMensaje.IsVisible = true;
            return;
        }

        var nuevo = new Movimiento
        {
            UsuarioId = Sesion.UsuarioActual?.Id ?? 0,
            Tipo = PickerTipo.SelectedItem.ToString() ?? "Ingreso",
            Categoria = categoriaSeleccionada.Nombre,
            Monto = monto,
            Fecha = DatePickerFecha.Date ?? DateTime.Today
        };

        if (nuevo.UsuarioId > 0)
        {
            try
            {
                await _databaseService.GuardarMovimientoAsync(nuevo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"No se pudo persistir el movimiento: {ex}");
                LabelMensaje.TextColor = Color.FromArgb("#FF8585");
                LabelMensaje.Text = "No se pudo guardar el movimiento. Inténtalo de nuevo.";
                LabelMensaje.IsVisible = true;
                return;
            }
        }

        Sesion.Movimientos.Add(nuevo);
        LabelMensaje.TextColor = Color.FromArgb("#68E0A3");
        LabelMensaje.Text = nuevo.UsuarioId <= 0
            ? "Guardado en esta sesión; no se conservará al salir."
            : "Movimiento guardado en tu cuenta.";
        LabelMensaje.IsVisible = true;

        EntryMonto.Text = string.Empty;
        DatePickerFecha.Date = DateTime.Today;
        PickerTipo.SelectedIndex = 0;
        CargarCategorias("Ingreso");

        ActualizarPreview();
    }
}

