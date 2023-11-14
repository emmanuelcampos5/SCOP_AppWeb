using System.ComponentModel.DataAnnotations;

namespace SCOP_AppWeb.Models
{
    public class OrdenProduccion
    {
        [Key]
        public int IdOrdenProduccion { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public string EstadoProduccion { get; set; }
        public int CantidadProductos { get; set; }
        public string Descripcion { get; set; }
        public bool EstadoActivo { get; set; }
    }
}
