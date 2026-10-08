namespace MovilL.Models;

public class Categoria
{
    public int Pk { get; set; }
    public int Id { get => Pk; set => Pk = value; }
    public string Nombre { get; set; } = string.Empty;
    public string Imagen { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Gasto"; // "Ingreso" o "Gasto"

    public Categoria()
    {
    }

    public Categoria(int pk, string nombre, string imagen, string tipo)
    {
        Pk = pk;
        Nombre = nombre;
        Imagen = imagen;
        Tipo = tipo;
    }

    // Registros en memoria sincronizados con las imágenes de Resources/Images
    public static List<Categoria> ListaCategorias { get; set; } = new()
    {
        // Ingresos
        new Categoria(1, "Sueldo", "sueldo.jpg", "Ingreso"),

        // Gastos
        new Categoria(2, "Comida", "comida.jpg", "Gasto"),
        new Categoria(3, "Transporte", "transporte.jpg", "Gasto"),
        new Categoria(4, "Arriendo", "arriendo.jpg", "Gasto"),
        new Categoria(5, "Servicios", "servicios.jpg", "Gasto"),
        new Categoria(6, "Estudio", "estudio.jpg", "Gasto")
    };
}

