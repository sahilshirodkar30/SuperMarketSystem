using System.ComponentModel.DataAnnotations;

namespace SuperMarketSystem.Server.Models
{
    public class LoginModel
    {

        [Required(ErrorMessage = "User Name Required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password Required")]
        public string Password { get; set; }
    }
}
