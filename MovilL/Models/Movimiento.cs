namespace MovilL.Models;

public class Movimiento
{
    public string Tipo { get; set; } = "";
    public string Categoria { get; set; } = "";
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }

    public string Icono => Categoria?.ToLower() switch
    {
        "sueldo" => "💵",
        "comida" => "🍔",
        "arriendo" => "🏠",
        "servicios" => "💡",
        "transporte" => "🚗",
        "estudio" or "estudios" => "📚",
        _ => Tipo?.Equals("Ingreso", StringComparison.OrdinalIgnoreCase) == true ? "💰" : "💸"
    };

    public Color ColorTipo => Tipo?.Equals("Ingreso", StringComparison.OrdinalIgnoreCase) == true 
        ? Color.FromArgb("#81C784") 
        : Color.FromArgb("#E57373");

    public string MontoFormateado => Tipo?.Equals("Ingreso", StringComparison.OrdinalIgnoreCase) == true
        ? $"+{Monto:C}"
        : $"-{Monto:C}";
}