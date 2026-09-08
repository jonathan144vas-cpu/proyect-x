using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControlViveresApp.Models
{
    public enum EstadoEntregaProgramada
    {
        [Display(Name = "Programada")] Programada = 0,
        [Display(Name = "Completada")] Completada = 1,
        [Display(Name = "Cancelada")] Cancelada = 2
    }

    /// <summary>
    /// Una entrega planeada hacia un lugar por visitar (departamento/municipio).
    /// Al completarla, se descuenta el inventario, se genera un comprobante en PDF
    /// y se registra el lugar en el historial de "Lugares Visitados".
    /// </summary>
    public class EntregaProgramada
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El lugar es obligatorio")]
        [StringLength(120, ErrorMessage = "El lugar no puede pasar de 120 caracteres")]
        [Display(Name = "Lugar o comunidad")]
        public string Lugar { get; set; } = string.Empty;

        [Required(ErrorMessage = "El departamento es obligatorio")]
        [StringLength(50)]
        [Display(Name = "Departamento")]
        public string Departamento { get; set; } = string.Empty;

        [Required(ErrorMessage = "El municipio es obligatorio")]
        [StringLength(80)]
        [Display(Name = "Municipio")]
        public string Municipio { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha programada es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha programada")]
        public DateOnly FechaProgramada { get; set; }

        [Range(0, 100000, ErrorMessage = "Las familias deben estar entre 0 y 100000")]
        [Display(Name = "Familias beneficiadas")]
        public int FamiliasBeneficiadas { get; set; }

        [Display(Name = "Estado")]
        public EstadoEntregaProgramada Estado { get; set; } = EstadoEntregaProgramada.Programada;

        [StringLength(400, ErrorMessage = "Las observaciones no pueden pasar de 400 caracteres")]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [StringLength(100)]
        [Display(Name = "Registrado por")]
        public string? RegistradoPor { get; set; }

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }

        [Display(Name = "Fecha de finalización")]
        public DateTime? FechaCompletada { get; set; }

        public List<DetalleEntregaProgramada> Detalles { get; set; } = [];
    }
}
