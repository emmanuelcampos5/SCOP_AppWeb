using System.ComponentModel.DataAnnotations;

namespace SCOP_AppWeb.Models
{   
    public class Requisiciones
    {
        [Key]
        public int IdRequisicion { get; set; }
        public int IdOrdenProduccion { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string TipoRequisicion { get; set; }
        public double CostoRequisicion { get; set; }
        public bool EstadoActivo { get; set; }
    }
}
