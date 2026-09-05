using System;
using System.ComponentModel.DataAnnotations;

namespace ControlViveresApp.Models
{
    public enum PrioridadPedido
    {
        [Display(Name = "Baja")] Baja = 0,
        [Display(Name = "Normal")] Normal = 1,
        [Display(Name = "Alta")] Alta = 2
    }

    public enum EstadoPedido
    {
        [Display(Name = "Pendiente")] Pendiente = 0,
        [Display(Name = "Solicitado")] Solicitado = 1,
        [Display(Name = "Recibido")] Recibido = 2,
        [Display(Name = "Cancelado")] Cancelado = 3
    }

    /// <summary>
    /// Un víver que hace falta comprar. Al marcarlo como recibido, su cantidad
    /// se suma automáticamente al inventario.
    /// </summary>
    public class Pedido
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del artículo es obligatorio")]
        [StringLength(100, ErrorMessage = "El artículo no puede pasar de 100 caracteres")]
        [Display(Name = "Artículo")]
        public string Articulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es obligatoria")]
        [StringLength(50)]
        [Display(Name = "Categoría")]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, 100000, ErrorMessage = "La cantidad debe estar entre 1 y 100000")]
        [Display(Name = "Cantidad solicitada")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "La unidad de medida es obligatoria")]
        [StringLength(30)]
        [Display(Name = "Unidad de medida")]
        public string UnidadMedida { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "El proveedor no puede pasar de 100 caracteres")]
        [Display(Name = "Proveedor")]
        public string? Proveedor { get; set; }

        [StringLength(120, ErrorMessage = "El destino no puede pasar de 120 caracteres")]
        [Display(Name = "Destino")]
        public string? Destino { get; set; }

        [StringLength(50)]
        [Display(Name = "Departamento")]
        public string? Departamento { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Se necesita para")]
        public DateOnly? FechaNecesaria { get; set; }

        [Display(Name = "Prioridad")]
        public PrioridadPedido Prioridad { get; set; } = PrioridadPedido.Normal;

        [Display(Name = "Estado")]
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

        [StringLength(400, ErrorMessage = "Las observaciones no pueden pasar de 400 caracteres")]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        // Datos de auditoría: se llenan solos, no salen en el formulario.
        [Display(Name = "Fecha de solicitud")]
        public DateTime FechaSolicitud { get; set; }

        [StringLength(100)]
        [Display(Name = "Solicitado por")]
        public string? SolicitadoPor { get; set; }
    }
}
