namespace Devoted.Business.Error
{
    public class ItemNotFoundOrNullError : Exception
    {
        public ItemNotFoundOrNullError(string message) : base(message)
        {
        }
    }
}
