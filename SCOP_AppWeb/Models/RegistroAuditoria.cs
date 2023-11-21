using System.ComponentModel.DataAnnotations;

namespace SCOP_AppWeb.Models
{
    public class RegistroAuditoria
    {
        [Key]
        public int IdAuditoria { get; set; }
        public string TablaModificada { get; set; }
        public DateTime FechaModificacion { get; set; }
        public int IdUsuarioModificacion { get; set; }
        public string Descripcion { get; set; }
    }
}
