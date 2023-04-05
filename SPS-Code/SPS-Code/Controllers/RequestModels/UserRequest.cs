namespace SPS_Code.Controllers.RequestModels
{
    public class UserRequest : RequestBase
    {
        public string Password { get; set; }
        public string Email { get; set; }
    }

    public class RegisterRequest : UserRequest
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordCheck { get; set; }
    }

    public class UserEditRequest
    {
        public string? Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? CategoryID { get; set; }
    }

    public class UserPasswordRequest
    {
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordCheck { get; set; }
    }
}
