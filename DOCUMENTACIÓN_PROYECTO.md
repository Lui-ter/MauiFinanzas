# 📱 RESUMEN DETALLADO DEL PROYECTO MOVILL - GESTIÓN FINANCIERA PERSONAL

## 🎯 PROPÓSITO GENERAL DEL PROYECTO
MovilL es una aplicación de **Gestión Financiera Personal** desarrollada con **.NET MAUI** 
(Multi-platform App UI) que permite a los usuarios registrar y monitorear sus ingresos y gastos.
La app está diseñada para funcionar en múltiples plataformas: Android, iOS, Windows y macOS.

---

## 📂 ESTRUCTURA DEL PROYECTO

```
MovilL/
├── Models/                    # Clases de datos y lógica de negocio
│   ├── Sesion.cs             # Gestión de sesión del usuario
│   └── Movimiento.cs          # Modelo de transacciones financieras
│
├── views/                      # Páginas XAML de la interfaz
│   ├── LoginPage.xaml/.cs     # Pantalla de autenticación
│   ├── NuevoMovimientoPage.xaml/.cs  # Formulario para agregar movimientos
│   └── ResumenPage.xaml/.cs   # Panel de resumen y estadísticas
│
├── Resources/                  # Recursos compartidos
│   ├── Styles/                # Estilos globales (Colors.xaml, Styles.xaml)
│   ├── Fonts/                 # Fuentes personalizadas
│   ├── Images/                # Imágenes de la aplicación
│   └── Splash/                # Pantalla de bienvenida
│
├── Platforms/                 # Código específico de cada plataforma
│   ├── Android/               # Configuración para Android
│   ├── iOS/                   # Configuración para iOS
│   ├── Windows/               # Configuración para Windows
│   └── MacCatalyst/           # Configuración para Mac
│
├── App.xaml/.cs               # Archivo raíz de la aplicación
├── AppShell.xaml/.cs          # Definición de navegación de la app
└── MauiProgram.cs             # Configuración inicial de MAUI
```

---

## 📋 DESCRIPCIÓN DETALLADA DE CADA ARCHIVO

### 🔧 ARCHIVOS DE CONFIGURACIÓN PRINCIPAL

#### 1. **MauiProgram.cs** - Inicializador de la Aplicación
**Ubicación:** `MovilL/MauiProgram.cs`
**Propósito:** Punto de entrada central que configura toda la aplicación MAUI

```csharp
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()                    // Establece App como principal
			.ConfigureFonts(fonts =>              // Carga las fuentes personalizadas
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();               // Activa logs en modo Debug
#endif

		return builder.Build();
	}
}
```

**¿Qué hace?**
- Crea y configura la aplicación MAUI
- Registra las fuentes que se usarán en toda la app
- Habilita la depuración en modo DEBUG

**Personalización:**
- Aquí puedes agregar servicios de inyección de dependencias
- Configurar temas globales
- Registrar bases de datos

---

#### 2. **App.xaml y App.xaml.cs** - Contenedor Principal
**Ubicación:** `MovilL/App.xaml` y `MovilL/App.xaml.cs`

**App.xaml** (Interfaz declarativa):
```xml
<?xml version="1.0" encoding="utf-8" ?>
<Application
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="MovilL.App">
	<Application.Resources>
		<!-- Recursos globales de la aplicación -->
	</Application.Resources>
</Application>
```

**App.xaml.cs** (Lógica):
```csharp
public partial class App : Application
{
	public App()
	{
		InitializeComponent();  // Carga los recursos XAML
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());  // Define AppShell como shell principal
	}
}
```

**¿Qué hace?**
- Define recursos globales (colores, estilos, etc.)
- Crea la ventana principal y especifica la navegación

**Personalización:**
- Aquí puedes definir temas globales
- Recursos compartidos por toda la app
- Colores y estilos por defecto

---

#### 3. **AppShell.xaml y AppShell.xaml.cs** - Navegación
**Ubicación:** `MovilL/AppShell.xaml`

**AppShell.xaml** (Define la estructura de navegación):
```xml
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	   xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	   xmlns:views="clr-namespace:MovilL.views"
	   x:Class="MovilL.AppShell"
	   FlyoutBehavior="Disabled">

	<ShellContent Title="Login"
				  Route="LoginPage"
				  ContentTemplate="{DataTemplate views:LoginPage}" />

	<TabBar Route="MainTabs">
		<ShellContent Title="Nuevo"
					  Route="NuevoMovimientoPage"
					  Icon="add.png"
					  ContentTemplate="{DataTemplate views:NuevoMovimientoPage}" />
		<ShellContent Title="Resumen"
					  Route="ResumenPage"
					  Icon="chart.png"
					  ContentTemplate="{DataTemplate views:ResumenPage}" />
	</TabBar>
</Shell>
```

**¿Qué hace?**
- Define cómo navegan entre pantallas
- Crea una estructura de pestaña con "Nuevo" y "Resumen"
- Define las rutas de navegación

**Personalización:**
- Puedes agregar más pestaña
- Cambiar el comportamiento del menú lateral (FlyoutBehavior)
- Agregar iconos personalizados

---

### 📦 MODELOS DE DATOS (Models)

#### 4. **Movimiento.cs** - Modelo de Transacción
**Ubicación:** `MovilL/Models/Movimiento.cs`

```csharp
namespace MovilL.Models;

public class Movimiento
{
	public string Tipo { get; set; } = "";          // "Ingreso" o "Gasto"
	public string Categoria { get; set; } = "";     // Ej: "Comida", "Sueldo"
	public decimal Monto { get; set; }              // Cantidad de dinero
	public DateTime Fecha { get; set; }             // Fecha del movimiento
}
```

**¿Qué es?**
- Representa una transacción financiera (ingreso o gasto)
- Almacena datos estructurados de cada movimiento

**Personalización:**
- Puedes agregar propiedades como "Descripción", "Notas", "ImagenRecibo"
- Agregar métodos de validación
- Cambiar tipos de datos

---

#### 5. **Sesion.cs** - Gestor de Sesión
**Ubicación:** `MovilL/Models/Sesion.cs`

```csharp
using System.Collections.Generic;
using MovilL.Models;

public static class Sesion
{
	// Estado de autenticación
	public static bool EstaLogueado { get; set; } = false;
	public static bool EsInvitado { get; set; } = false;
	public static string NombreUsuario { get; set; } = "";

	// Lista de movimientos en memoria
	// Si es invitado, se llena y se pierde al cerrar la app
	// Si es usuario, más adelante esto se puede cambiar por SQLite/Preferences
	public static List<Movimiento> Movimientos { get; set; } = new();

	public static void CerrarSesion()
	{
		EstaLogueado = false;
		EsInvitado = false;
		NombreUsuario = "";
		Movimientos.Clear();
	}
}
```

**¿Qué hace?**
- Almacena datos de sesión del usuario actual
- Guarda la lista de todos los movimientos financieros
- Es una clase ESTÁTICA, accesible desde cualquier parte de la app

**Personalización:**
- Cambiar de almacenamiento en memoria a SQLite o Preferences
- Agregar más propiedades de usuario (email, teléfono, etc.)
- Implementar autenticación real con servidor

---

### 🎨 PÁGINAS/VISTAS (Views)

#### 6. **LoginPage.xaml y LoginPage.xaml.cs** - Pantalla de Autenticación
**Ubicación:** `MovilL/views/LoginPage.xaml` y `.xaml.cs`

**LoginPage.xaml** (Interfaz):
```xml
<ContentPage ...>
	<VerticalStackLayout Padding="30" Spacing="20" VerticalOptions="Center">

		<Label Text="Gestión Financiera Personal"
			   FontSize="24" FontAttributes="Bold" />

		<!-- Campo de usuario -->
		<Entry x:Name="EntryUsuario" Placeholder="Usuario" />

		<!-- Campo de contraseña -->
		<Entry x:Name="EntryContrasena" Placeholder="Contraseña" IsPassword="True" />

		<!-- Mensaje de error -->
		<Label x:Name="LabelError" TextColor="Red" IsVisible="False"
			   Text="Usuario o contraseña incorrectos" />

		<!-- Botón de login -->
		<Button Text="Iniciar sesión"
				Clicked="OnIniciarSesionClicked"
				BackgroundColor="#4CAF50" TextColor="White" />

		<!-- Botón de invitado -->
		<Button Text="Entrar como invitado"
				Clicked="OnEntrarInvitadoClicked"
				BackgroundColor="Transparent" TextColor="#4CAF50" />
	</VerticalStackLayout>
</ContentPage>
```

**LoginPage.xaml.cs** (Lógica):
```csharp
public partial class LoginPage : ContentPage
{
	private const string UsuarioValido = "admin";
	private const string ContrasenaValida = "1234";

	public LoginPage()
	{
		InitializeComponent();  // Carga la interfaz XAML
	}

	// Evento: Botón "Iniciar sesión"
	private async void OnIniciarSesionClicked(object sender, EventArgs e)
	{
		// Validar credenciales (actualmente quemadas)
		if (EntryUsuario.Text == UsuarioValido && EntryContrasena.Text == ContrasenaValida)
		{
			// Guardar estado en Sesion
			Sesion.EstaLogueado = true;
			Sesion.EsInvitado = false;
			Sesion.NombreUsuario = UsuarioValido;

			// Navegar a NuevoMovimientoPage
			await Shell.Current.GoToAsync("//NuevoMovimientoPage");
		}
		else
		{
			LabelError.IsVisible = true;  // Mostrar error
		}
	}

	// Evento: Botón "Entrar como invitado"
	private async void OnEntrarInvitadoClicked(object sender, EventArgs e)
	{
		// Marcar como invitado
		Sesion.EstaLogueado = true;
		Sesion.EsInvitado = true;
		Sesion.NombreUsuario = "Invitado";

		// Navegar a la página de movimientos
		await Shell.Current.GoToAsync("//NuevoMovimientoPage");
	}
}
```

**¿Qué hace?**
- Pantalla inicial de la app
- Permite login con credenciales o modo invitado
- Valida usuario/contraseña y establece sesión

**IMPORTANTE - Credenciales actuales:**
- Usuario: `admin`
- Contraseña: `1234`

**Personalización:**
- Reemplazar credenciales quemadas con autenticación real
- Conectar con un servidor/API
- Agregar validación de email
- Implementar registro de usuarios

---

#### 7. **NuevoMovimientoPage.xaml y NuevoMovimientoPage.xaml.cs** - Formulario de Movimiento
**Ubicación:** `MovilL/views/NuevoMovimientoPage.xaml` y `.xaml.cs`

**NuevoMovimientoPage.xaml** (Interfaz - Formulario):
```xml
<ContentPage Title="Nuevo movimiento">
	<ScrollView>
		<VerticalStackLayout Padding="20" Spacing="15">

			<!-- Selector de tipo -->
			<Label Text="Tipo de movimiento" FontAttributes="Bold" />
			<Picker x:Name="PickerTipo" Title="Selecciona">
				<Picker.Items>
					<x:String>Ingreso</x:String>
					<x:String>Gasto</x:String>
				</Picker.Items>
			</Picker>

			<!-- Campo de categoría -->
			<Label Text="Categoría" FontAttributes="Bold" />
			<Entry x:Name="EntryCategoria" 
				   Placeholder="Ej: Comida, Sueldo, Transporte" />

			<!-- Campo de monto -->
			<Label Text="Monto" FontAttributes="Bold" />
			<Entry x:Name="EntryMonto" Placeholder="0.00" Keyboard="Numeric" />

			<!-- Selector de fecha -->
			<Label Text="Fecha" FontAttributes="Bold" />
			<DatePicker x:Name="DatePickerFecha" />

			<!-- Botón guardar -->
			<Button Text="Guardar movimiento"
					Clicked="OnGuardarClicked"
					BackgroundColor="#4CAF50" TextColor="White" />

			<!-- Mensaje de estado -->
			<Label x:Name="LabelMensaje" TextColor="Green" IsVisible="False" />
		</VerticalStackLayout>
	</ScrollView>
</ContentPage>
```

**NuevoMovimientoPage.xaml.cs** (Lógica - Guardar Movimiento):
```csharp
public partial class NuevoMovimientoPage : ContentPage
{
	public NuevoMovimientoPage()
	{
		InitializeComponent();
	}

	// Evento: Botón "Guardar movimiento"
	private void OnGuardarClicked(object sender, EventArgs e)
	{
		// Validar que tipo y categoría estén completos
		if (PickerTipo.SelectedItem == null || 
			string.IsNullOrWhiteSpace(EntryCategoria.Text))
		{
			LabelMensaje.TextColor = Colors.Red;
			LabelMensaje.Text = "Completa tipo y categoría antes de guardar.";
			LabelMensaje.IsVisible = true;
			return;
		}

		// Crear nuevo movimiento
		var nuevo = new Movimiento
		{
			Tipo = PickerTipo.SelectedItem.ToString(),           // "Ingreso" o "Gasto"
			Categoria = EntryCategoria.Text,                     // Categoría ingresada
			Monto = decimal.TryParse(EntryMonto.Text, out var monto) ? monto : 0,  // Monto
			Fecha = DatePickerFecha.Date ?? DateTime.Today       // Fecha (hoy si no selecciona)
		};

		// Guardar en la lista de sesión
		Sesion.Movimientos.Add(nuevo);

		// Mostrar mensaje de éxito
		LabelMensaje.TextColor = Colors.Green;
		LabelMensaje.Text = Sesion.EsInvitado
			? "Guardado (como invitado, no se conservará al salir)."
			: "Movimiento guardado correctamente.";
		LabelMensaje.IsVisible = true;

		// Limpiar el formulario
		PickerTipo.SelectedItem = null;
		EntryCategoria.Text = "";
		EntryMonto.Text = "";
		DatePickerFecha.Date = DateTime.Today;
	}
}
```

**¿Qué hace?**
- Formulario para registrar nuevos ingresos o gastos
- Valida que los campos requeridos estén completos
- Guarda el movimiento en la sesión
- Muestra mensajes de éxito o error

**Personalización:**
- Agregar más categorías predefinidas
- Subir foto de recibo
- Agregar notas/descripción
- Guardar en base de datos en lugar de sesión

---

#### 8. **ResumenPage.xaml y ResumenPage.xaml.cs** - Panel de Resumen
**Ubicación:** `MovilL/views/ResumenPage.xaml` y `.xaml.cs`

**ResumenPage.xaml** (Interfaz - Estadísticas):
```xml
<ContentPage Title="Resumen">
	<Grid RowDefinitions="Auto,Auto,Auto,*" Padding="20" RowSpacing="15">

		<!-- Botón cerrar sesión -->
		<Button Grid.Row="0" Text="Cerrar sesión"
				Clicked="OnCerrarSesionClicked"
				BackgroundColor="Transparent" TextColor="Red" HorizontalOptions="End" />

		<!-- Fila: Ingresos y Gastos -->
		<Grid Grid.Row="1" ColumnDefinitions="*,*" ColumnSpacing="10">

			<!-- Tarjeta de Ingresos -->
			<Frame Grid.Column="0" BackgroundColor="#E8F5E9" CornerRadius="10">
				<VerticalStackLayout>
					<Label Text="Ingresos" FontSize="14" />
					<Label x:Name="LabelIngresos" Text="$0" 
						   FontSize="22" FontAttributes="Bold" TextColor="Green" />
				</VerticalStackLayout>
			</Frame>

			<!-- Tarjeta de Gastos -->
			<Frame Grid.Column="1" BackgroundColor="#FFEBEE" CornerRadius="10">
				<VerticalStackLayout>
					<Label Text="Gastos" FontSize="14" />
					<Label x:Name="LabelGastos" Text="$0" 
						   FontSize="22" FontAttributes="Bold" TextColor="Red" />
				</VerticalStackLayout>
			</Frame>
		</Grid>

		<!-- Balance Total -->
		<Frame Grid.Row="2" BackgroundColor="#E3F2FD" CornerRadius="10">
			<VerticalStackLayout>
				<Label Text="Balance" FontSize="14" />
				<Label x:Name="LabelBalance" Text="$0" 
					   FontSize="22" FontAttributes="Bold" TextColor="Blue" />
			</VerticalStackLayout>
		</Frame>

		<!-- Lista de Movimientos -->
		<CollectionView Grid.Row="3" x:Name="ListaMovimientos"
						SelectionMode="Single">
			<CollectionView.ItemTemplate>
				<DataTemplate>
					<StackLayout Padding="10" Spacing="5">
						<Label Text="{Binding Categoria}" FontSize="16" FontAttributes="Bold" />
						<Label Text="{Binding Fecha, StringFormat='{0:dd/MM/yyyy}'}" FontSize="12" />
						<Label Text="{Binding Monto, StringFormat='${0:F2}'}" 
							   TextColor="{Binding Tipo, Converter={...}}" />
					</StackLayout>
				</DataTemplate>
			</CollectionView.ItemTemplate>
		</CollectionView>
	</Grid>
</ContentPage>
```

**ResumenPage.xaml.cs** (Lógica - Cálculos):
```csharp
public partial class ResumenPage : ContentPage
{
	public ResumenPage()
	{
		InitializeComponent();
	}

	// Se ejecuta cuando la página aparece
	protected override void OnAppearing()
	{
		base.OnAppearing();
		ActualizarResumen();  // Recalcular al volver a esta página
	}

	private void ActualizarResumen()
	{
		// Sumar ingresos
		var ingresos = Sesion.Movimientos
			.Where(m => m.Tipo == "Ingreso")
			.Sum(m => m.Monto);

		// Sumar gastos
		var gastos = Sesion.Movimientos
			.Where(m => m.Tipo == "Gasto")
			.Sum(m => m.Monto);

		// Actualizar etiquetas
		LabelIngresos.Text = ingresos.ToString("C");        // Formato moneda: $1000.50
		LabelGastos.Text = gastos.ToString("C");
		LabelBalance.Text = (ingresos - gastos).ToString("C");

		// Mostrar lista de movimientos
		ListaMovimientos.ItemsSource = Sesion.Movimientos;
	}

	// Evento: Botón "Cerrar sesión"
	private async void OnCerrarSesionClicked(object sender, EventArgs e)
	{
		Sesion.CerrarSesion();  // Limpiar datos de sesión
		await Shell.Current.GoToAsync("//LoginPage");  // Volver a login
	}
}
```

**¿Qué hace?**
- Muestra resumen de ingresos, gastos y balance
- Lista todos los movimientos registrados
- Calcula totales automáticamente
- Permite cerrar sesión

**Personalización:**
- Agregar gráficos de estadísticas
- Filtrar por fecha o categoría
- Exportar a PDF o Excel
- Mostrar gráficos de tendencias

---

## 🔄 FLUJO DE LA APLICACIÓN

```
┌─────────────────────────────────────────────────────────┐
│                1. INICIO (MauiProgram.cs)               │
│         Carga fuentes y configura la app                │
└────────────────────┬────────────────────────────────────┘
					 │
┌────────────────────▼────────────────────────────────────┐
│            2. PANTALLA DE LOGIN (LoginPage)             │
│    ┌─ Usuario + Contraseña ──► Backend/Validar         │
│    │                                                     │
│    └─ Opción "Invitado" ──► Sesión Temporal            │
└────────────────────┬────────────────────────────────────┘
					 │
		┌────────────┴────────────┐
		│                         │
┌───────▼──────────┐  ┌──────────▼─────────┐
│ NUEVO MOVIMIENTO │  │  RESUMEN Y ESTADIS │
│  (Agregar datos) │  │  (Ver resumen)     │
│                  │  │ (Cerrar sesión)    │
└────────┬─────────┘  └───────┬────────────┘
		 │                    │
		 └────────┬───────────┘
				  │
		 ┌────────▼──────────┐
		 │ Guardar en Sesion │
		 │  (En memoria)     │
		 └───────────────────┘
```

---

## 💾 FLUJO DE DATOS

```
┌──────────────────────────────────────────────────────────┐
│          SESION (Clase Estática Global)                 │
│ ┌────────────────────────────────────────────────────┐  │
│ │ - EstaLogueado: bool                               │  │
│ │ - EsInvitado: bool                                 │  │
│ │ - NombreUsuario: string                            │  │
│ │ - Movimientos: List<Movimiento>  ◄─── Todos acceden│  │
│ │ - CerrarSesion(): void                             │  │
│ └────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────┘
		  ▲                              ▲
		  │                              │
		  │                              │
	┌─────┴──────────────┐      ┌────────┴──────────────┐
	│ NuevoMovimientoPage│      │   ResumenPage        │
	│   (Agregar datos)  │      │  (Consultar datos)   │
	│                    │      │                      │
	│ .Add(nuevo)        │      │ .Where() .Sum()      │
	└────────────────────┘      └──────────────────────┘
```

---

## 🎨 COLORES Y ESTILOS UTILIZADOS

**Color Primario (Verde):** `#4CAF50` (Botones principales)
**Verde oscuro:** `#2E7D32`
**Rojo (errores):** `Red` o `#FF5252`
**Azul (Balance):** `Blue` o `#2196F3`
**Fondos:**
  - Ingresos: `#E8F5E9` (Verde claro)
  - Gastos: `#FFEBEE` (Rojo claro)
  - Balance: `#E3F2FD` (Azul claro)

---

## 🔐 SEGURIDAD Y DATOS

**IMPORTANTE:**
- Las credenciales están **quemadas** en el código (admin/1234)
- Los datos se guardan **EN MEMORIA** solamente
- Para invitados, los datos se pierden al cerrar la app
- **NO es seguro para producción**

**Mejoras necesarias:**
1. Usar autenticación real (API, OAuth, Firebase)
2. Cifrar contraseñas
3. Guardar en SQLite o Preferences
4. Sincronizar con servidor

---

## 🚀 PRÓXIMOS PASOS PARA PERSONALIZAR

### 1. **Cambiar Colores**
   - Edita `Resources/Styles/Colors.xaml`
   - Actualiza los códigos HEX de colores

### 2. **Agregar Base de Datos**
   - Integra SQLite con Entity Framework Core
   - Reemplaza Sesion.Movimientos por consultas a BD

### 3. **Autenticación Real**
   - Conecta con un servidor/API
   - Reemplaza credenciales quemadas

### 4. **Agregar Categorías**
   - Crea tabla de categorías
   - Carga dinámicamente en NuevoMovimientoPage

### 5. **Reportes y Gráficos**
   - Integra charting library (Maui.Charts)
   - Crea gráficos de ingresos vs gastos

### 6. **Sincronización**
   - Sincroniza datos entre dispositivos
   - Guarda en la nube

---

## 📚 REFERENCIAS Y COMANDOS ÚTILES

**Compilar proyecto:**
```bash
dotnet build MovilL.csproj
```

**Ejecutar en Android:**
```bash
dotnet maui run -f net10.0-android
```

**Ejecutar en Windows:**
```bash
dotnet maui run -f net10.0-windows10.0.19041.0
```

**Limpiar proyecto:**
```bash
dotnet clean
```

---

## ✅ CHECKLIST ANTES DE USAR

- [ ] Cambiar credenciales de login
- [ ] Configurar conexión a servidor/API
- [ ] Implementar base de datos
- [ ] Actualizar colores y branding
- [ ] Probar en múltiples plataformas
- [ ] Agregar validaciones adicionales
- [ ] Implementar manejo de errores
- [ ] Agregar logs y debugging

---

## 📞 NOTAS IMPORTANTES

1. **Namespace:** `MovilL` (principal), `MovilL.Models`, `MovilL.views`
2. **Estilo de código:** C# 11 con características modernas
3. **Plataformas soportadas:** Android, iOS, Windows, macOS
4. **Framework:** .NET 10 con MAUI
5. **Almacenamiento:** Actualmente en memoria (Sesion.cs)

---

**FIN DEL RESUMEN - Proyecto MovilL v1.0**
