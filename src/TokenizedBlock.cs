using System.Text;

namespace SimpleContentLanguage
{
    public class TokenizedBlock
    {
        private readonly List<TokenizedLine> _tokenizedLines;

        public IReadOnlyList<TokenizedLine> TokenizedLines => _tokenizedLines;

        public TokenizedBlock(List<TokenizedLine> tokenizedLines)
        {
            _tokenizedLines = tokenizedLines ?? new List<TokenizedLine>();
        }

        public bool TryGetNextTokenInBounds(Token token, TokenBounds tokenBounds, out Token nextToken)
        {
            nextToken = default;

            if (!tokenBounds.IsTokenInBounds(token))
                return false;

            if (_tokenizedLines == null || _tokenizedLines.Count == 0)
                return false;

            bool passedCurrent = false;

            foreach (var line in _tokenizedLines)
            {
                if (line?.Tokens == null || line.Tokens.Count == 0)
                    continue;

                foreach (var t in line.Tokens)
                {
                    if (!tokenBounds.IsTokenInBounds(t))
                        continue;

                    if (!passedCurrent)
                    {
                        if (IsSamePosition(t, token))
                            passedCurrent = true;

                        continue;
                    }

                    nextToken = t;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetPreviousTokenInBounds(Token token, TokenBounds tokenBounds, out Token previousToken)
        {
            previousToken = default;

            if (!tokenBounds.IsTokenInBounds(token))
                return false;

            if (_tokenizedLines == null || _tokenizedLines.Count == 0)
                return false;

            Token candidate = default;
            bool hasCandidate = false;

            foreach (var line in _tokenizedLines)
            {
                if (line?.Tokens == null || line.Tokens.Count == 0)
                    continue;

                foreach (var t in line.Tokens)
                {
                    if (!tokenBounds.IsTokenInBounds(t))
                        continue;

                    if (IsSamePosition(t, token))
                    {
                        if (!hasCandidate)
                            return false;

                        previousToken = candidate;
                        return true;
                    }

                    candidate = t;
                    hasCandidate = true;
                }
            }

            return false;
        }

        public string CreateMergedMetaLinesInBounds(TokenBounds tokenBounds)
        {
            StringBuilder content = new StringBuilder();
            bool isPreviousLineEmpty = true;
            string previousLine = string.Empty;

            foreach (string line in GetMetaLinesInBounds(tokenBounds))
            {
                if (!isPreviousLineEmpty)
                {
                    content.AppendLine(previousLine);
                }

                previousLine = line;
                isPreviousLineEmpty = false;
            }
            content.Append(previousLine);

            return content.ToString();
        }

        public IEnumerable<string> GetMetaLinesInBounds(TokenBounds tokenBounds)
        {
            if (_tokenizedLines == null || _tokenizedLines.Count == 0)
                yield break;

            foreach (TokenizedLine tokenizedLine in _tokenizedLines)
            {
                if (tokenizedLine == null)
                    continue;

                int metaLineId = tokenizedLine.MetaLineId;
                if (metaLineId < tokenBounds.StartToken.MetaLineId || metaLineId > tokenBounds.EndToken.MetaLineId)
                    continue;

                yield return tokenizedLine.GetLineInBounds(tokenBounds);
            }
        }

        public IEnumerable<Token> GetTokensInBounds(TokenBounds tokenBounds)
        {
            if (_tokenizedLines == null || _tokenizedLines.Count == 0)
                yield break;

            foreach (var line in _tokenizedLines)
            {
                if (line?.Tokens == null || line.Tokens.Count == 0)
                    continue;

                foreach (var token in line.Tokens)
                {
                    if (tokenBounds.IsTokenInBounds(token))
                        yield return token;
                }
            }
        }

        private static bool IsSamePosition(Token a, Token b)
            => a.MetaLineId == b.MetaLineId && a.TokenId == b.TokenId;
    }
}
