namespace SimpleContentLanguage
{
    public class ArgsLineRecognizer : IElementRecognizer
    {
        public readonly string StartToken;

        public ArgsLineRecognizer(string startToken)
        {
            StartToken = startToken;
        }

        public virtual bool IsStart(Token token)
        {
            return token.Value.Equals(StartToken);
        }

        public virtual bool IsEnd(Token token)
        {
            return token.IsLastInLine;
        }
    }
}
