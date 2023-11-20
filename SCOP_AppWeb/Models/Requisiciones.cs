using System;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;

namespace SCOP_AppWeb.Models
{
    public class Requisiciones
    {
        [Key]
        public int IdRequisicion { get; set; }

        [Display(Name = "ID de Orden de Producción")]
        public int IdOrdenProduccion { get; set; }

        [Display(Name = "ID de Usuario")]
        public int IdUsuario { get; set; }

        [Display(Name = "Fecha de Creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Tipo de Requisición")]
        [Required(ErrorMessage = "El tipo de requisición es obligatorio.")]
        public string TipoRequisicion { get; set; }

        [Display(Name = "Costo")]
        [Required(ErrorMessage = "Por favor ingrese el costo.")]
        public double CostoRequisicion { get; set; }

        [Display(Name = "Estado Activo")]
        public bool EstadoActivo { get; set; }
    }
}
