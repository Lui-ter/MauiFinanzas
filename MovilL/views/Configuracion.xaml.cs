using MovilL.Models;
using MovilL.Services;

namespace MovilL.views;

public partial class Configuracion : ContentPage
{
    private readonly IMovilDatabaseService _databaseService;
    private Categoria? _categoriaEnEdicion;
    private byte[]? _nuevaImagen;
    private string _extensionImagen = ".png";
    private string _imagenOriginal = string.Empty;
    private bool _quitarImagen;

    public Configuracion(IMovilDatabaseService databaseService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        InitializeComponent();
        PickerFiltro.SelectedIndex = 0;
        PickerTipoEditor.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        MenuUsuario.CargarUsuario();
        ToolbarUsuario.IconImageSource = Sesion.UsuarioActual?.Imagen ?? "user.png";
        LabelAccesoCategorias.IsVisible = (Sesion.UsuarioActual?.Id ?? 0) <= 0;
        await CargarCategoriasAsync();
    }

    private async Task CargarCategoriasAsync()
    {
        try
        {
            Sesion.Categorias = await _databaseService.GetCategoriasAsync(Sesion.UsuarioActual?.Id ?? 0);
            ActualizarListaCategorias();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudieron cargar las categorías: {ex}");
            await DisplayAlertAsync("Error", "No se pudieron cargar tus categorías. Inténtalo de nuevo.", "Aceptar");
        }
    }

    private void ActualizarListaCategorias()
    {
        string? filtro = PickerFiltro.SelectedIndex switch
        {
            1 => "Ingreso",
            2 => "Gasto",
            _ => null
        };

        var categorias = Sesion.Categorias
            .Where(c => filtro == null || c.Tipo.Equals(filtro, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.Tipo)
            .ThenBy(c => c.UsuarioId)
            .ThenBy(c => c.Nombre)
            .ToList();

        BindableLayout.SetItemsSource(ListaCategorias, categorias);
        LabelSinCategorias.IsVisible = categorias.Count == 0;
        LabelResumenCategorias.Text = categorias.Count == 1
            ? "1 categoría disponible"
            : $"{categorias.Count} categorías disponibles";
    }

    private void OnFiltroChanged(object? sender, EventArgs e)
    {
        ActualizarListaCategorias();
    }

    private async void OnUsuarioClicked(object? sender, EventArgs e)
    {
        await MenuUsuario.AbrirCerrarMenu();
    }

    private async void OnCrearCategoriaClicked(object? sender, EventArgs e)
    {
        _categoriaEnEdicion = null;
        _imagenOriginal = string.Empty;
        _nuevaImagen = null;
        _quitarImagen = false;
        LabelTituloEditor.Text = "Crear tipo de movimiento";
        BtnGuardarCategoria.Text = "Guardar categoría";
        EntryNombreCategoria.Text = string.Empty;
        PickerTipoEditor.SelectedIndex = 0;
        MostrarImagen(null);
        LabelMensajeCategoria.IsVisible = false;
        bool puedeGuardar = (Sesion.UsuarioActual?.Id ?? 0) > 0;
        BtnGuardarCategoria.IsEnabled = puedeGuardar;
        LabelMensajeCategoria.Text = puedeGuardar
            ? string.Empty
            : "Inicia sesión con una cuenta para crear categorías personales.";
        LabelMensajeCategoria.TextColor = Color.FromArgb("#FFC66D");
        LabelMensajeCategoria.IsVisible = !puedeGuardar;
        await EnfocarEditorAsync();
    }

    private async void OnEditarCategoriaClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is not Categoria categoria)
            return;

        int usuarioId = Sesion.UsuarioActual?.Id ?? 0;
        if (usuarioId <= 0)
        {
            await DisplayAlertAsync("Inicia sesión", "Necesitas una cuenta para personalizar categorías.", "Aceptar");
            return;
        }
        if (categoria.UsuarioId != 0 && categoria.UsuarioId != usuarioId)
            return;

        _categoriaEnEdicion = categoria;
        _imagenOriginal = categoria.Imagen;
        _nuevaImagen = null;
        _quitarImagen = false;
        LabelTituloEditor.Text = "Personalizar categoría";
        BtnGuardarCategoria.Text = "Guardar cambios";
        EntryNombreCategoria.Text = categoria.Nombre;
        PickerTipoEditor.SelectedIndex = categoria.Tipo.Equals("Gasto", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        MostrarImagen(string.IsNullOrWhiteSpace(categoria.Imagen) ? null : categoria.Imagen);
        LabelMensajeCategoria.IsVisible = false;
        BtnGuardarCategoria.IsEnabled = true;
        await EnfocarEditorAsync();
    }

    private async Task EnfocarEditorAsync()
    {
        PanelEditorCategoria.TranslationY = 24;
        PanelEditorCategoria.Opacity = 0;
        PanelEditorCategoria.IsVisible = true;

        await Task.Yield();
        Task desplazamiento = ScrollConfiguracion.ScrollToAsync(
            PanelEditorCategoria,
            ScrollToPosition.Start,
            animated: true);
        Task animacion = Task.WhenAll(
            PanelEditorCategoria.TranslateToAsync(0, 0, 260, Easing.CubicOut),
            PanelEditorCategoria.FadeToAsync(1, 220, Easing.CubicOut));

        await Task.WhenAll(desplazamiento, animacion);
        EntryNombreCategoria.Focus();
    }

    private async void OnSeleccionarImagenClicked(object? sender, EventArgs e)
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
                MostrarMensaje("Selecciona una imagen JPG o PNG.", Color.FromArgb("#FF8585"));
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
                    MostrarMensaje("La imagen no puede superar los 10 MB.", Color.FromArgb("#FF8585"));
                    return;
                }

                await memory.WriteAsync(buffer, 0, bytesRead);
            }

            _nuevaImagen = memory.ToArray();
            _extensionImagen = extension.ToLowerInvariant();
            _quitarImagen = false;
            MostrarImagenSource(ImageSource.FromStream(() => new MemoryStream(_nuevaImagen)));
            LabelMensajeCategoria.IsVisible = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudo seleccionar la imagen de categoría: {ex}");
            MostrarMensaje("No se pudo cargar la imagen seleccionada.", Color.FromArgb("#FF8585"));
        }
    }

    private void OnQuitarImagenClicked(object? sender, EventArgs e)
    {
        _nuevaImagen = null;
        _quitarImagen = true;
        MostrarImagen(null);
    }

    private async void OnGuardarCategoriaClicked(object? sender, EventArgs e)
    {
        int usuarioId = Sesion.UsuarioActual?.Id ?? 0;
        if (usuarioId <= 0)
        {
            MostrarMensaje("Inicia sesión con una cuenta para guardar categorías personales.", Color.FromArgb("#FF8585"));
            return;
        }

        string nombre = EntryNombreCategoria.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nombre))
        {
            MostrarMensaje("Escribe un nombre para la categoría.", Color.FromArgb("#FF8585"));
            return;
        }

        string tipo = PickerTipoEditor.SelectedItem?.ToString() ?? "Ingreso";
        string imagenAnterior = _imagenOriginal;
        string? nuevaRutaImagen = null;
        string nuevaImagenPersistida = _quitarImagen ? string.Empty : imagenAnterior;

        try
        {
            if (_nuevaImagen != null)
            {
                string carpetaImagenes = Path.Combine(FileSystem.AppDataDirectory, "categorias");
                Directory.CreateDirectory(carpetaImagenes);
                nuevaRutaImagen = Path.Combine(carpetaImagenes, $"categoria-{Guid.NewGuid():N}{_extensionImagen}");
                await File.WriteAllBytesAsync(nuevaRutaImagen, _nuevaImagen);
                nuevaImagenPersistida = nuevaRutaImagen;
            }

            Categoria categoriaGuardada;
            if (_categoriaEnEdicion == null)
            {
                categoriaGuardada = await _databaseService.GuardarCategoriaAsync(new Categoria
                {
                    UsuarioId = usuarioId,
                    Nombre = nombre,
                    Tipo = tipo,
                    Imagen = nuevaImagenPersistida
                });
                Sesion.Categorias.Add(categoriaGuardada);
            }
            else
            {
                int categoriaOriginalId = _categoriaEnEdicion.Pk;
                categoriaGuardada = await _databaseService.ActualizarCategoriaAsync(new Categoria
                {
                    Pk = categoriaOriginalId,
                    UsuarioId = usuarioId,
                    Nombre = nombre,
                    Tipo = tipo,
                    Imagen = nuevaImagenPersistida
                }, usuarioId);

                int index = Sesion.Categorias.FindIndex(c => c.Pk == categoriaOriginalId);
                if (index >= 0)
                    Sesion.Categorias.RemoveAt(index);
                Sesion.Categorias.Add(categoriaGuardada);
            }
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(nuevaRutaImagen))
                TryEliminarImagenPersonal(nuevaRutaImagen);

            System.Diagnostics.Debug.WriteLine($"No se pudo guardar la categoría: {ex}");
            string mensaje = ex is InvalidOperationException
                ? ex.Message
                : "No se pudo guardar la categoría. Comprueba la conexión con la base de datos.";
            MostrarMensaje(mensaje, Color.FromArgb("#FF8585"));
            return;
        }

        bool imagenAnteriorEliminada = string.Equals(imagenAnterior, nuevaImagenPersistida, StringComparison.Ordinal) ||
                                       string.IsNullOrWhiteSpace(imagenAnterior) ||
                                       TryEliminarImagenPersonal(imagenAnterior);
        ActualizarListaCategorias();
        PanelEditorCategoria.IsVisible = false;
        await DisplayAlertAsync(
            "Categoría guardada",
            imagenAnteriorEliminada
                ? "Tu categoría quedó actualizada y solo estará disponible en tu cuenta."
                : "La categoría quedó guardada, pero no se pudo limpiar la imagen anterior del almacenamiento.",
            "Aceptar");
    }

    private async void OnEliminarCategoriaClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is not Categoria categoria)
            return;

        int usuarioId = Sesion.UsuarioActual?.Id ?? 0;
        if (usuarioId <= 0)
        {
            await DisplayAlertAsync("Inicia sesión", "Necesitas una cuenta para borrar categorías.", "Aceptar");
            return;
        }
        if (categoria.UsuarioId != 0 && categoria.UsuarioId != usuarioId)
            return;

        bool confirmar = await DisplayAlertAsync(
            "Borrar categoría",
            $"¿Quieres quitar «{categoria.Nombre}» de tus categorías? Los movimientos guardados con anterioridad se conservarán.",
            "Borrar",
            "Cancelar");
        if (!confirmar)
            return;

        try
        {
            bool eliminada = await _databaseService.EliminarCategoriaAsync(categoria.Pk, usuarioId);
            if (!eliminada)
            {
                await DisplayAlertAsync("No se pudo borrar", "La categoría ya no existe o no pertenece a tu cuenta.", "Aceptar");
                await CargarCategoriasAsync();
                return;
            }

            Sesion.Categorias.RemoveAll(c => c.Pk == categoria.Pk);
            ActualizarListaCategorias();
            bool imagenEliminada = string.IsNullOrWhiteSpace(categoria.Imagen) || TryEliminarImagenPersonal(categoria.Imagen);
            await DisplayAlertAsync(
                "Categoría borrada",
                imagenEliminada
                    ? "La categoría se borró. El historial de movimientos no se modificó."
                    : "La categoría se borró, pero no se pudo limpiar su imagen del almacenamiento.",
                "Aceptar");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudo borrar la categoría: {ex}");
            await DisplayAlertAsync("Error", "No se pudo borrar la categoría. Inténtalo de nuevo.", "Aceptar");
        }
    }

    private void OnCancelarEditorClicked(object? sender, EventArgs e)
    {
        PanelEditorCategoria.IsVisible = false;
        _categoriaEnEdicion = null;
        _nuevaImagen = null;
    }

    private void MostrarImagen(string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
        {
            ImagePreviewCategoria.Source = null;
            ImagePreviewCategoria.IsVisible = false;
            LabelSinImagen.IsVisible = true;
            return;
        }

        ImagePreviewCategoria.Source = ruta;
        ImagePreviewCategoria.IsVisible = true;
        LabelSinImagen.IsVisible = false;
    }

    private void MostrarImagenSource(ImageSource? source)
    {
        ImagePreviewCategoria.Source = source;
        ImagePreviewCategoria.IsVisible = source != null;
        LabelSinImagen.IsVisible = source == null;
    }

    private void MostrarMensaje(string mensaje, Color color)
    {
        LabelMensajeCategoria.Text = mensaje;
        LabelMensajeCategoria.TextColor = color;
        LabelMensajeCategoria.IsVisible = true;
    }

    private static bool TryEliminarImagenPersonal(string ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
            return true;

        string directorioCategorias;
        string rutaCompleta;
        try
        {
            directorioCategorias = Path.GetFullPath(Path.Combine(FileSystem.AppDataDirectory, "categorias"));
            rutaCompleta = Path.GetFullPath(ruta);
        }
        catch (ArgumentException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ruta de imagen no válida: {ex}");
            return false;
        }
        catch (NotSupportedException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ruta de imagen no válida: {ex}");
            return false;
        }
        catch (PathTooLongException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ruta de imagen no válida: {ex}");
            return false;
        }

        string prefijoPermitido = directorioCategorias.EndsWith(Path.DirectorySeparatorChar)
            ? directorioCategorias
            : directorioCategorias + Path.DirectorySeparatorChar;
        StringComparison comparacionRuta = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        if (!rutaCompleta.StartsWith(prefijoPermitido, comparacionRuta))
            return true;

        try
        {
            if (File.Exists(rutaCompleta))
                File.Delete(rutaCompleta);
            return true;
        }
        catch (IOException ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudo limpiar imagen de categoría: {ex}");
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudo limpiar imagen de categoría: {ex}");
            return false;
        }
    }
}
