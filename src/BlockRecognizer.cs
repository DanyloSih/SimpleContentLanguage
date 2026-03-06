namespace SimpleContentLanguage
{
    public class BlockRecognizer : IElementRecognizer
    {
        public readonly string StartToken;
        public readonly string EndToken;

        public BlockRecognizer(string startToken, string endToken)
        {
            StartToken = startToken;
            EndToken = endToken;
        }

        public virtual bool IsStart(Token token)
        {
            return token.Text.Equals(StartToken);
        }

        public virtual bool IsEnd(Token token)
        {
            return token.Text.Equals(EndToken);
        }
    }
}
