namespace SimpleContentLanguage
{
    public class Result
    {
        public readonly bool IsSuccessful;
        public readonly string ErrorMessage;

        public Result(bool isSuccessful, string errorMessage)
        {
            IsSuccessful = isSuccessful;
            ErrorMessage = errorMessage;
        }
    }
}
