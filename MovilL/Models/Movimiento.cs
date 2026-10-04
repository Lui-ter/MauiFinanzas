namespace MovilL.Models;

public class Movimiento
{
    public string Tipo { get; set; } = "";
    public string Categoria { get; set; } = "";
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
}