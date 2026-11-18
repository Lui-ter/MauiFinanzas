# Documentación del proyecto PetShop MAUI

## Propósito

Este documento describe la estructura y el funcionamiento actual de PetShop,
una aplicación .NET MAUI para administrar clientes, productos, categorías y
compras. También resume cómo se usan MVVM, comandos, inyección de dependencias
y SQLite, para que estos patrones puedan servir de contexto al trabajar en
otro proyecto.

La documentación se basa en el código actual y en los temas de estas
presentaciones:

- **Clase 10 - Commands**, diapositivas 7–22.
- **Clase 11 - Patron MVVM**, diapositivas 5–39.
- **Clase 14 - MAUI y SQLite**, diapositivas 6–17.

Las diapositivas explican los conceptos; este proyecto conserva algunas
decisiones propias. Por ejemplo, la raíz visual activa usa `FlyoutPage` y
`NavigationPage`, y la persistencia SQLite es local: no hay sincronización con
un backend HTTP.

## 1. Tecnologías

| Tecnología | Uso |
| --- | --- |
| .NET MAUI / .NET 10 | Interfaz multiplataforma. El proyecto declara destinos Android, iOS, Mac Catalyst y Windows, según el sistema de compilación. |
| XAML | Estructura declarativa de páginas y enlaces de datos. |
| MVVM | Separación progresiva entre presentación, estado y lógica de pantalla. |
| CommunityToolkit.Mvvm | `ObservableObject`, `[ObservableProperty]` y `AsyncRelayCommand`. |
| Entity Framework Core | Contexto, entidades y consultas a datos. |
| SQLite | Base local `ShopPet.db3`. |
| DI de .NET MAUI | Construcción e inyección de vistas, ViewModels y servicios. |

Paquetes principales de [`petshop.csproj`](petshop.csproj):

- `CommunityToolkit.Mvvm` 8.4.2.
- `Microsoft.EntityFrameworkCore` 10.0.11.
- `Microsoft.EntityFrameworkCore.Sqlite` 10.0.11.

## 2. Estructura del código fuente

El árbol siguiente omite artefactos generados de `bin/` y `obj/`.

```text
petshop/
├── DbContext/
│   └── DbContext .cs
├── Handler/
│   ├── CategoriaBusquedaHandler.xaml(.cs)
│   ├── ClienteBusquedaHandler.xaml(.cs)
│   └── ProductoBusquedaHandler.xaml(.cs)
├── Services/
│   ├── DatabasePathService.cs
│   ├── IDatabasePathService.cs
│   ├── INavegacionService.cs
│   └── NavegacionService.cs
├── ViewModels/
│   ├── ComprasViewModel.cs
│   ├── HelpSupportDetailViewModel.cs
│   └── HelpSupportViewModel.cs
├── Views/
│   ├── About.xaml(.cs)
│   ├── AboutPage.xaml(.cs)
│   ├── MainContainer.xaml(.cs)
│   ├── MainPage.xaml(.cs)
│   ├── HelpSupportPage.xaml(.cs)
│   ├── HelpSupportDetailPage.xaml(.cs)
│   ├── ComprasPage.xaml(.cs)
│   ├── Cliente.xaml(.cs), DetalleCliente.xaml(.cs)
│   ├── Producto.xaml(.cs), DetalleProducto.xaml(.cs)
│   └── Categoria.xaml(.cs), DetalleCategoria.xaml(.cs)
├── Platforms/
├── Resources/
├── App.xaml(.cs)
├── AppShell.xaml(.cs)
├── MauiProgram.cs
├── MiComando.cs
├── ViewModelGlobal.cs
└── petshop.csproj
```

### `Views/`: interfaz

Las páginas MAUI suelen consistir en un XAML y su code-behind `.xaml.cs`.
XAML define controles y bindings; el code-behind inicializa la página y, en
algunas pantallas antiguas, maneja eventos.

- [`HelpSupportPage.xaml`](Views/HelpSupportPage.xaml) presenta clientes.
- [`HelpSupportDetailPage.xaml`](Views/HelpSupportDetailPage.xaml) permite
  elegir producto, cambiar la cantidad, pulsar **Agregar**, ver la lista actual
  y pulsar **Finalizar Compra**.
- [`ComprasPage.xaml`](Views/ComprasPage.xaml) muestra registros consultados de
  SQLite con cliente, producto, cantidad y valores calculados.
- [`About.xaml`](Views/About.xaml) define el menú lateral, donde aparece
  **Compras**.
- [`MainContainer.xaml`](Views/MainContainer.xaml) tiene la navegación inferior
  para Home, Clientes, Productos y Categorías.

[`HelpSupportPage.xaml.cs`](Views/HelpSupportPage.xaml.cs) y
[`HelpSupportDetailPage.xaml.cs`](Views/HelpSupportDetailPage.xaml.cs) reciben
sus ViewModels por constructor y asignan `BindingContext`.

No todas las pantallas siguen MVVM todavía: hay pantallas y handlers
existentes que conservan lógica en code-behind. Los flujos de HelpSupport y
Compras son los ejemplos principales de la separación aplicada.

### `ViewModels/`: estado y lógica de pantalla

Los ViewModels exponen propiedades y comandos para la vista. No deberían
guardar referencias a controles visuales ni modificar directamente el árbol de
interfaz.

- [`HelpSupportViewModel.cs`](ViewModels/HelpSupportViewModel.cs) carga la
  colección de clientes y solicita ir al detalle a través de
  `INavegacionService`.
- [`HelpSupportDetailViewModel.cs`](ViewModels/HelpSupportDetailViewModel.cs)
  recibe el cliente de la navegación, carga productos y compras, mantiene la
  selección y cantidad, y guarda una compra al ejecutar **Agregar**.
- [`ComprasViewModel.cs`](ViewModels/ComprasViewModel.cs) consulta compras,
  productos y clientes de la base y crea elementos de presentación con nombres
  y totales.
- [`ViewModelGlobal.cs`](ViewModelGlobal.cs) es una base abstracta que deriva
  de `ObservableObject` de CommunityToolkit.Mvvm.

`[ObservableProperty]` permite generar propiedades observables y notificar a
la vista cuando cambia su valor. Para usar estos miembros generados, las
clases ViewModel se declaran `partial`.

### `MiComando.cs`: implementación propia de `ICommand`

[`MiComando.cs`](MiComando.cs) implementa `ICommand` utilizando:

- `Action` para la operación que ejecuta el comando.
- `Func<bool>` para determinar si se puede ejecutar.
- `RaiseCanExecuteChanged()` para notificar que la condición de ejecución debe
  evaluarse de nuevo.

`HelpSupportDetailViewModel` emplea `MiComando` para:

- `AddCommand`: validar y guardar una compra.
- `IncreaseQuantityCommand`: incrementar la cantidad hasta 100.
- `DecreaseQuantityCommand`: reducir la cantidad hasta un mínimo de 1.

`FinalizarCompraCommand` es otro tipo de comando: usa `AsyncRelayCommand` del
Toolkit porque solicita navegación asíncrona.

### `Services/`: navegación y ruta de SQLite

- [`INavegacionService.cs`](Services/INavegacionService.cs) declara las
  operaciones de navegación requeridas.
- [`NavegacionService.cs`](Services/NavegacionService.cs) implementa la
  navegación sobre el `NavigationPage` que está dentro del `FlyoutPage`.
- [`IDatabasePathService.cs`](Services/IDatabasePathService.cs) define el
  contrato para recuperar la ruta de base de datos.
- [`DatabasePathService.cs`](Services/DatabasePathService.cs) construye la ruta
  multiplataforma de `ShopPet.db3` mediante `FileSystem.AppDataDirectory`.

La interfaz de navegación evita acoplar el ViewModel directamente a
`Shell.Current` o a los controles de navegación de la interfaz.

### `DbContext/`: entidades y acceso a datos

El archivo [`DbContext .cs`](DbContext/DbContext%20.cs) define
`ShopDbContext`, las entidades y los conjuntos de datos:

| DbSet | Contenido |
| --- | --- |
| `Categories` | Categorías de productos. |
| `Products` | Productos y sus datos descriptivos y precio. |
| `Clients` | Clientes de la tienda. |
| `Compras` | Cliente, producto y cantidad de cada registro de compra. |

`OnModelCreating` configura datos iniciales de categorías, productos y
clientes mediante `HasData`. `OnConfiguring` configura SQLite como respaldo
cuando el contexto se crea sin opciones; la aplicación normalmente lo obtiene
de `IDbContextFactory<ShopDbContext>` con sus opciones registradas en DI.

Los ViewModels crean un contexto para leer o guardar datos y lo disponen al
terminar la operación. Así no mantienen un contexto EF de larga duración
asociado a una página.

### `MauiProgram.cs`: configuración e inyección de dependencias

[`MauiProgram.cs`](MauiProgram.cs) configura la aplicación y registra
dependencias, entre ellas:

- `INavegacionService` con `NavegacionService` como singleton.
- `IDatabasePathService` con `DatabasePathService` como singleton.
- `IDbContextFactory<ShopDbContext>` configurada para SQLite.
- Páginas y ViewModels como servicios transitorios.

Al iniciar, la aplicación ejecuta `Database.EnsureCreated()` para crear la
base si no existe.

`EnsureCreated()` es una solución simple para crear la base durante el
desarrollo; no actualiza automáticamente un esquema existente. Si el esquema
cambia y se deben conservar datos, utiliza migraciones de EF Core y una
estrategia de actualización apropiada.

## 3. MVVM y data binding

El `BindingContext` de una página determina dónde se resuelven los nombres de
los bindings. Por ejemplo, el XAML puede enlazar una etiqueta y un botón con
propiedades del ViewModel:

```xml
<Label Text="{Binding Cantidad}" />
<Button Text="Agregar" Command="{Binding AddCommand}" />
```

Flujo del binding:

1. DI crea la vista y le entrega su ViewModel.
2. La vista asigna el ViewModel a `BindingContext`.
3. MAUI resuelve `Cantidad` y `AddCommand` sobre ese contexto.
4. Una propiedad observable notifica a la vista cuando cambia.
5. El botón consulta `CanExecute`; si es verdadero, ejecuta el comando.

Si un binding apunta a un nombre incorrecto, la aplicación puede compilar y
aun así no mostrar el valor esperado. Conviene verificar los nombres del XAML
y las propiedades públicas del ViewModel.

## 4. Flujo de compra

El flujo actual mantiene separados **Agregar** y **Finalizar Compra**:

1. Desde HelpSupport se elige un cliente y se abre `HelpSupportDetailPage`.
2. `NavegacionService` entrega el identificador al ViewModel mediante
   `ApplyQueryAttributes`.
3. El usuario elige producto y cantidad.
4. `AddCommand` verifica que haya un cliente válido, un producto seleccionado y
   una cantidad positiva.
5. `AgregarCompra()` crea una entidad `Compra`, la inserta en SQLite y ejecuta
   `SaveChanges()`.
6. La colección de compras del detalle se actualiza para mostrar el nuevo
   registro en esa pantalla.
7. `FinalizarCompraCommand` navega a la página de consulta de compras.
8. `ComprasViewModel` vuelve a leer los datos persistidos y `ComprasPage` los
   presenta.

**Alcance actual:** la compra queda persistida al pulsar **Agregar**.
**Finalizar Compra** abre la vista de compras; no agrega otra fila, no vacía un
carrito y no procesa un pago. Si otro sistema requiere una confirmación o un
pago, debe modelar esos pasos por separado.

La opción **Compras** del menú lateral abre esa vista de consulta desde el
contenedor principal. La acción desde el detalle también puede navegar a ella.

## 5. SQLite: archivo y consultas

El nombre de archivo es `ShopPet.db3`; MAUI lo guarda en el directorio
privado indicado por `FileSystem.AppDataDirectory`. La ruta depende de la
plataforma. No se debe fijar una ruta local de una computadora en el código
compartido.

Para localizar el archivo en Windows desde PowerShell:

```powershell
Get-ChildItem "$env:LOCALAPPDATA" -Filter ShopPet.db3 -Recurse -File `
  -ErrorAction SilentlyContinue |
  Select-Object -ExpandProperty FullName
```

Con `sqlite3` disponible en el `PATH`, asigna a `$db` la ruta localizada:

```powershell
$db = 'C:\ruta\encontrada\ShopPet.db3'
sqlite3 $db ".tables"
sqlite3 -header -column $db "SELECT * FROM Compras;"
```

Ejemplo de consulta que incluye los nombres de cliente y producto:

```sql
SELECT
    c.Id,
    cl.Nombre || ' ' || cl.Apellido AS Cliente,
    p.Nombre AS Producto,
    c.Cantidad,
    p.Precio AS PrecioUnitario,
    c.Cantidad * p.Precio AS Total
FROM Compras AS c
JOIN Clients AS cl ON cl.Id = c.ClientId
JOIN Products AS p ON p.Id = c.ProductId;
```

La vista **Compras** realiza su lectura desde la aplicación y no depende de la
instalación de la herramienta `sqlite3`.

## 6. Navegación de la aplicación

`App.xaml.cs` crea `About` como raíz activa. `About` hereda de `FlyoutPage`,
contiene el menú de tres líneas y aloja una `NavigationPage` cuyo contenido
inicial es `MainContainer`.

Hay dos zonas de navegación visibles:

- El menú lateral: Home, Acerca de mi Tienda, HelpSupport y Compras.
- La barra inferior de `MainContainer`: Home, Clientes, Productos y
  Categorías.

`AppShell.xaml` existe en el repositorio, pero no es la raíz activa. No se debe
asumir que la navegación del proyecto usa `Shell.Current`; la implementación
actual conserva la estructura `FlyoutPage`/`NavigationPage`.

## 7. Guía para reutilizar el patrón

1. Define las entidades y los casos de uso antes de diseñar la interfaz.
2. Crea un `DbContext` con un `DbSet` por entidad y configura el proveedor de
   almacenamiento.
3. Centraliza la ruta de SQLite con `FileSystem.AppDataDirectory`.
4. Registra el contexto o su factoría, las vistas, ViewModels y servicios en
   `MauiProgram.cs`.
5. Crea un ViewModel para el estado y las acciones propias de cada pantalla.
6. Enlaza XAML con propiedades observables y comandos del ViewModel.
7. Elige el comando que corresponda: un `ICommand` propio para practicar o
   extender su comportamiento, o los comandos del Toolkit para reducir código
   repetido.
8. Aísla la navegación en un servicio si el ViewModel necesita solicitar un
   cambio de página.
9. Lee de la base de datos en la vista de consulta; no dependas solo de una
   colección temporal en memoria.
10. Prueba persistencia cerrando y volviendo a abrir la pantalla o aplicación.
11. Si el esquema evoluciona, planifica migraciones para preservar datos.

Para una aplicación nueva no es necesario copiar literalmente todos los
archivos. Conserva los conceptos y adapta nombres, navegación, entidades y
ciclo de vida a los requisitos del otro proyecto.

## 8. Compilar y ejecutar en Windows

Desde el directorio que contiene `petshop.csproj`:

```powershell
dotnet build .\petshop.csproj -f net10.0-windows10.0.19041.0
dotnet build .\petshop.csproj -t:Run -f net10.0-windows10.0.19041.0
```

El segundo comando compila y ejecuta la aplicación. Para Android, el SDK debe
tener instalado y configurado el workload Android de MAUI:

```powershell
dotnet build .\petshop.csproj -f net10.0-android
```

## 9. Relación con las presentaciones

| Presentación | Diapositivas | Conceptos reflejados |
| --- | --- | --- |
| Clase 10 - Commands | 7–14, 16–22 | Binding a `ICommand`, clase `MiComando`, validación de `CanExecute`, entidad y colección de compras. |
| Clase 11 - Patron MVVM | 5–10, 13–27, 29–39 | ViewModels, separación de navegación, DI, Toolkit, propiedades observables, parámetros y vistas inyectadas. |
| Clase 14 - MAUI y SQLite | 6–13, 15–17 | Proveedor SQLite, servicio de ruta, contexto por factoría, persistencia y vista de consulta. |

### Diferencias entre la guía y esta aplicación

- La clase SQLite menciona sincronización con un backend; esta app no incluye
  una API ni sincroniza datos.
- La guía de navegación presenta `Shell`; la app activa utiliza
  `FlyoutPage`/`NavigationPage`.
- Las acciones síncronas del detalle usan `MiComando`; la navegación asíncrona
  de finalizar usa `AsyncRelayCommand`.
- La compra se guarda con **Agregar**. **Finalizar Compra** navega a la vista
  de consulta y no procesa pago.
- Otras áreas del proyecto mantienen pantallas y lógica previas que todavía no
  se migraron al flujo MVVM descrito.

