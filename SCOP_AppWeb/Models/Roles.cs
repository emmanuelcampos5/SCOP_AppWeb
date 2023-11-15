using System.ComponentModel.DataAnnotations;

namespace SCOP_AppWeb.Models
{
    public class Roles
    {
        [Key]
        public int idRol { get; set; }
        public string nombreRol { get; set; }
    }
}
