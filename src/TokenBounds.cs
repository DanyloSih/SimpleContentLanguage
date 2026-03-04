namespace SimpleContentLanguage
{
    public struct TokenBounds
    {
        public readonly Token StartToken;
        public readonly Token EndToken;

        public TokenBounds(Token startToken, Token endToken)
        {
            StartToken = startToken;
            EndToken = endToken;
        }

        public bool IsTokenInBounds(Token token)
        {
            if (token.SourceLineId < StartToken.SourceLineId ||
                token.SourceLineId > EndToken.SourceLineId)
            {
                return false;
            }

            if (token.SourceLineId == StartToken.SourceLineId &&
                token.TokenId < StartToken.TokenId)
            {
                return false;
            }

            if (token.SourceLineId == EndToken.SourceLineId &&
                token.TokenId > EndToken.TokenId)
            {
                return false;
            }

            return true;
        }
    }
}
