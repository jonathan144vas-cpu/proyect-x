using System;
using System.ComponentModel.DataAnnotations;

namespace ControlViveresApp.Models
{
    /// <summary>
    /// Un renglón del inventario de víveres.
    /// </summary>
    public class Alimento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del alimento es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede pasar de 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es obligatoria")]
        [StringLength(50, ErrorMessage = "La categoría no puede pasar de 50 caracteres")]
        [Display(Name = "Categoría")]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0, 100000, ErrorMessage = "La cantidad debe estar entre 0 y 100000")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "La unidad de medida es obligatoria")]
        [StringLength(30, ErrorMessage = "La unidad de medida no puede pasar de 30 caracteres")]
        [Display(Name = "Unidad de medida")]
        public string UnidadMedida { get; set; } = string.Empty;

        // DateOnly se guarda en Postgres como "date": es una fecha de calendario,
        // sin hora ni zona horaria, que es justo lo que es un vencimiento.
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de vencimiento")]
        public DateOnly? FechaVencimiento { get; set; }

        // Fecha en que el alimento entró al inventario. Se asigna una sola vez,
        // al crearlo, y no se vuelve a tocar al editar.
        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }
    }
}
