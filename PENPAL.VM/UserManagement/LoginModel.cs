using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.VM.UserManagement
{
    public class LoginModel
    {
        [RegularExpression("^[A-Za-z0-9_@.-]*$", ErrorMessage = "Please enter valid username")]
        [Required(ErrorMessage = "Please enter username")]
        public string LoginUserName { get; set; }

        [RegularExpression("^[A-Za-z0-9_@./#&+-]*$", ErrorMessage = "Please enter valid password")]
        [Required(ErrorMessage = "Please enter password")]
        public string LoginPassword { get; set; }

        public string Message { get; set; }


    }
}
