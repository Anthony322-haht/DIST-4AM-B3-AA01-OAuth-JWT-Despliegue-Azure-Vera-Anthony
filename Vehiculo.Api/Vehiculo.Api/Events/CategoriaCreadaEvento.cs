namespace Vehiculo.Api.Events
{
    public class CategoriaCreadaEvento
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }
    }
}
