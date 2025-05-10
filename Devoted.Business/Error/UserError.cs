namespace Devoted.Business.Error
{
    public class UserError : Exception
    {
        public UserError(string message) : base(message)
        {
        }
    }
}
