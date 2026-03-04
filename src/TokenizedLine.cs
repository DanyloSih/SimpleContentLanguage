namespace SimpleContentLanguage
{
    public class TokenizedLine
    {
        public readonly string OriginalLine;
        public readonly List<Token> Tokens;

        public TokenizedLine(string originalLine, List<Token> tokens)
        {
            OriginalLine = originalLine;
            Tokens = tokens;
        }
    }
}
