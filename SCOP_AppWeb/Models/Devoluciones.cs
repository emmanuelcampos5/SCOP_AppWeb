using System.ComponentModel.DataAnnotations;

namespace SCOP_AppWeb.Models
{
    public class Devoluciones
    {
        [Key]
        public int IdDevolucion { get; set; }
        public int IdOrdenProduccion { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public double ImporteDevolucion { get; set; }
        public bool EstadoActivo { get; set; }
    }
}
