using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Text;

namespace SCOP_AppWeb.Models
{
    public class EmailRegistro
    {
        public void Enviar(Usuarios usuario)
        {
            try
            {
                MailMessage email = new MailMessage();

                email.Subject = "Datos de registro en plataforma SCOP";

                email.To.Add(new MailAddress("SCOP_System@outlook.com"));

                email.To.Add(new MailAddress(usuario.correoUsuario));

                email.From = new MailAddress("SCOP_System@outlook.com");

                string html = "Etapa de Registro al sistema SCOP";
                html += "<br> A continuacion detallamos sus credenciales de inicio de sesion";
                html += "<br><b>ID Correo:</b>" + usuario.correoUsuario;
                html += "<br><b>Contraseña:</b>" + usuario.password;
                html += "<br><b>Es necesario que inicie sesion para que complete el registro</b>";
                html += "<br><b>Este correo generado de forma automatica, favor no responderlo.";
                html += "Por la plataforma web SCOP_System.</b>";

                email.IsBodyHtml = true;

                email.Priority = MailPriority.Normal;

                AlternateView view = AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, MediaTypeNames.Text.Html);

                email.AlternateViews.Add(view);

                SmtpClient smtp = new SmtpClient();

                smtp.Host = "smtp-mail.outlook.com";

                smtp.Port = 587;

                smtp.EnableSsl = true;

                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("SCOP_System@outlook.com", "Hotel123456");

                smtp.Send(email);

                email.Dispose();
                smtp.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
