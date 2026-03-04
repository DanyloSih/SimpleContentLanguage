namespace SimpleContentLanguage
{
    public class ResultWithValue<TValue> : Result
    {
        public readonly TValue? Value;

        public ResultWithValue(
            bool isSuccessful,
            string errorMessage,
            TValue? value) : base(isSuccessful, errorMessage)
        {
            Value = value;
        }
    }
}
