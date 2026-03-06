namespace SimpleContentLanguage
{
    public class TokenizedLine
    {
        public readonly string MetaLine;
        public readonly int MetaLineId;

        private readonly List<Token> _tokens;

        public IReadOnlyList<Token> Tokens { get => _tokens; }

        public TokenizedLine(string metaLine, int metaLineId, List<Token> tokens)
        {
            MetaLine = metaLine;
            MetaLineId = metaLineId;
            _tokens = tokens;
        }

        public IEnumerable<Token> GetTokensInBounds(TokenBounds tokenBounds)
        {
            foreach (Token token in Tokens)
            {
                if (tokenBounds.IsTokenInBounds(token))
                {
                    yield return token;
                }
            }
        }

        public string GetLineInBounds(TokenBounds tokenBounds)
        {
            if (string.IsNullOrEmpty(MetaLine))
                return string.Empty;

            if (Tokens == null || Tokens.Count == 0)
                return MetaLine;

            int lineId = Tokens[0].MetaLineId;

            if (tokenBounds.StartToken.MetaLineId > lineId)
                return string.Empty;

            if (tokenBounds.EndToken.MetaLineId < lineId)
                return string.Empty;

            bool startOnThisLine = tokenBounds.StartToken.MetaLineId == lineId;
            bool endOnThisLine = tokenBounds.EndToken.MetaLineId == lineId;

            if (!startOnThisLine && !endOnThisLine)
                return MetaLine;

            int startIndex = 0;
            int endIndexExclusive = MetaLine.Length;

            if (startOnThisLine)
            {
                int position = tokenBounds.StartToken.FirstCharPositionInMetaLine;
                startIndex = Math.Clamp(position, 0, MetaLine.Length);
            }

            if (endOnThisLine)
            {
                int position = tokenBounds.EndToken.FirstCharPositionInMetaLine;
                int length = tokenBounds.EndToken.Text?.Length ?? 0;
                endIndexExclusive = Math.Clamp(position + length, 0, MetaLine.Length);
            }

            if (endIndexExclusive <= startIndex)
                return string.Empty;

            return MetaLine.Substring(startIndex, endIndexExclusive - startIndex);
        }
    }
}
