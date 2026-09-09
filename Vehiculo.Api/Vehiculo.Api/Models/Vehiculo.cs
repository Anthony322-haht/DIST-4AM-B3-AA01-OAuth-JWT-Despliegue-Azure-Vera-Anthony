using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Vehiculo.Api.Models
{
    [Table("Vehiculo")]
    public class Vehiculo
    {
        [Key]
        [Column("IdVehiculo")]
        public int IdVehiculo { get; set; }

        [Required]
        [Column("IdCategoria")]
        public int IdCategoria { get; set; }
        [Required]
        [StringLength(100)]
        [Column("Marca")]

        public string Marca { get; set; } = string.Empty;
        [Required]

        [StringLength(100)]
        [Column("Modelo")]
        public string Modelo { get; set; } = string.Empty;
        [Column("Precio", TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }
        [Column("Stock")]
        public int Stock { get; set; } = 0;
        [Column("Estado")]

        public bool Estado { get; set; } = true;
    }
}
