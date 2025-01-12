using System.ComponentModel.DataAnnotations;

namespace SuperMarketSystem.Server.Models
{
    public class SignUpModel
    {
        [Required(ErrorMessage = "Username is Required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Email is Required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is Required")]
        public string Password { get; set; }
    }
}
