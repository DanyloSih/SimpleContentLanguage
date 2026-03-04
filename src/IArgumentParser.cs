namespace SimpleContentLanguage
{
    public interface IArgumentParser<TResult>
    {
        public ResultWithValue<TResult> Parse(string argumentText);
    }
}
