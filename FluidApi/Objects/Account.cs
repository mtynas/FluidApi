using System;
namespace FluidAutomationService.Objects
{
    public class Account
    {

        public Int64 AccountNo { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Password { get; set; }
        public string SessionId { get; set; }


        public Account( )
        {
        }
    }
}
