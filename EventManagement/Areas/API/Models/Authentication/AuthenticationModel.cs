namespace EventManagement.Areas.API.Models.Authentication
{
#pragma warning disable CS0436 // Type conflicts with imported type
    public class AuthenticationModel : BaseModel
#pragma warning restore CS0436 // Type conflicts with imported type
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Code { get; set; }

        public AuthenticationModel()
        {
            Email = string.Empty;
            Password = string.Empty;
            Code = string.Empty;
        }
    }
}