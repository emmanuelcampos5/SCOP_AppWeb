using System.ComponentModel.DataAnnotations;

namespace SCOP_AppWeb.Models
{      
    public class Usuarios
    {
        [Key]
        public int idUsuario { get; set; }
        public int idRol { get; set; }
        public string nombreUsuario { get; set; }
        public string correoUsuario { get; set; }
        public string telefonoUsuario { get; set; }
        public string password { get; set; }
        public bool estadoActivo { get; set; }
    }
}
