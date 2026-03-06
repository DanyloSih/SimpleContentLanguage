namespace SimpleContentLanguage
{
    public interface IArgumentParser<TResult>
    {
        public ResultWithValue<TResult> Parse(Token argumentToken);
    }
}
