using System;
using System.ComponentModel.DataAnnotations;

namespace SCOP_AppWeb.Models
{
    public class Devoluciones
    {
        [Key]
        [Display(Name = "ID de Devolución")]
        public int IdDevolucion { get; set; }

        [Display(Name = "ID de Orden de Producción")]        
        public int IdOrdenProduccion { get; set; }

        [Display(Name = "ID de Usuario")]        
        public int IdUsuario { get; set; }

        [Display(Name = "Fecha de Creación")]       
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Importe de Devolución")]
        [Required(ErrorMessage = "El campo Importe de Devolucion es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El Importe de Devolucion debe ser un valor positivo")]
        public double ImporteDevolucion { get; set; }

        [Display(Name = "Estado Activo")]
        public bool EstadoActivo { get; set; }
    }
}
