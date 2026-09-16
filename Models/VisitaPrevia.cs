using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControlViveresApp.Models
{
    public enum EstadoVisitaPrevia
    {
        [Display(Name = "Pendiente")] Pendiente = 0,
        [Display(Name = "Aprobada")] Aprobada = 1,
        [Display(Name = "Rechazada")] Rechazada = 2
    }

    /// <summary>
    /// Revisión previa de una posible visita a un lugar, antes de programarla
    /// formalmente. Al aprobarla se genera una Entrega Programada con los
    /// mismos datos y productos propuestos.
    /// </summary>
    public class VisitaPrevia
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

        [Required(ErrorMessage = "La fecha propuesta es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha propuesta")]
        public DateOnly FechaPropuesta { get; set; }

        [Range(0, 100000, ErrorMessage = "Las familias deben estar entre 0 y 100000")]
        [Display(Name = "Familias beneficiadas")]
        public int FamiliasBeneficiadas { get; set; }

        [Display(Name = "Estado")]
        public EstadoVisitaPrevia Estado { get; set; } = EstadoVisitaPrevia.Pendiente;

        [StringLength(400, ErrorMessage = "Las observaciones no pueden pasar de 400 caracteres")]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [StringLength(100)]
        [Display(Name = "Registrado por")]
        public string? RegistradoPor { get; set; }

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }

        [Display(Name = "Fecha de revisión")]
        public DateTime? FechaRevision { get; set; }

        // Se llena cuando se aprueba y se genera la Entrega Programada correspondiente.
        public int? EntregaProgramadaGeneradaId { get; set; }

        public List<DetalleVisitaPrevia> Detalles { get; set; } = [];
    }
}
