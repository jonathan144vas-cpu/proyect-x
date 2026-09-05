using System;
using System.ComponentModel.DataAnnotations;

namespace ControlViveresApp.Models
{
    /// <summary>
    /// Un lugar de Guatemala al que ya se fue a dejar víveres.
    /// Guarda latitud y longitud para poder dibujar el mapa más adelante.
    /// </summary>
    public class Entrega
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del lugar es obligatorio")]
        [StringLength(120, ErrorMessage = "El lugar no puede pasar de 120 caracteres")]
        [Display(Name = "Lugar o comunidad")]
        public string Lugar { get; set; } = string.Empty;

        [Required(ErrorMessage = "El departamento es obligatorio")]
        [StringLength(50)]
        [Display(Name = "Departamento")]
        public string Departamento { get; set; } = string.Empty;

        [StringLength(80, ErrorMessage = "El municipio no puede pasar de 80 caracteres")]
        [Display(Name = "Municipio")]
        public string? Municipio { get; set; }

        [Required(ErrorMessage = "La fecha de la entrega es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de la entrega")]
        public DateOnly FechaEntrega { get; set; }

        [Range(0, 100000, ErrorMessage = "Las familias deben estar entre 0 y 100000")]
        [Display(Name = "Familias beneficiadas")]
        public int FamiliasBeneficiadas { get; set; }

        [Range(0, 1000000, ErrorMessage = "La cantidad debe estar entre 0 y 1000000")]
        [Display(Name = "Total de víveres entregados")]
        public int TotalEntregado { get; set; }

        // Se llenan solas con el departamento elegido, pero se pueden ajustar a mano
        // si algún día se quiere marcar la aldea exacta en el mapa.
        [Range(-90, 90)]
        [Display(Name = "Latitud")]
        public double? Latitud { get; set; }

        [Range(-180, 180)]
        [Display(Name = "Longitud")]
        public double? Longitud { get; set; }

        [StringLength(400, ErrorMessage = "Las observaciones no pueden pasar de 400 caracteres")]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [StringLength(100)]
        [Display(Name = "Registrado por")]
        public string? RegistradoPor { get; set; }

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }
    }
}
