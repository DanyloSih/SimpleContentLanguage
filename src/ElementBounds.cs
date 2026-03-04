namespace SimpleContentLanguage
{
    public struct ElementBounds
    {
        public readonly Token StartToken;
        public readonly Token EndToken;

        public ElementBounds(Token startToken, Token endToken)
        {
            StartToken = startToken;
            EndToken = endToken;
        }
    }
}
