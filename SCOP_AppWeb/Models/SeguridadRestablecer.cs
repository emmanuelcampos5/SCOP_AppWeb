using System.ComponentModel.DataAnnotations;

namespace SCOP_AppWeb.Models
{
    public class SeguridadRestablecer
    {
        public string correoUsuario { get; set; }

        public string nombreUsuario { get; set; }

        public string telefonoUsuario { get; set; }

        [Required(ErrorMessage = "Digite la contraseña enviada por email")]
        [DataType(DataType.Password)]
        public string password { get; set; }


        [Required(ErrorMessage = "Digite la nueva contraseña")]
        [DataType(DataType.Password)]
        public string nuevoPassword { get; set; }


        [Required(ErrorMessage = "Confirme la contraseña")]
        [DataType(DataType.Password)]
        public string confirmar { get; set; }
    }
}
