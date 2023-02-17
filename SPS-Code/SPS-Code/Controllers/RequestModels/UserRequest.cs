namespace SPS_Code.Controllers.RequestModels
{
    public class UserRequest : ModelBase
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
}
