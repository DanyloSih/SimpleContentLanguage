using System.Text;

namespace SimpleContentLanguage
{
    public class Tokenizer
    {
        public readonly char StringDelimiter;
        public readonly char EscapeChar;
        public readonly string MissingDelimeterErrorMessageFormat;

        /// <param name="stringDelimiter">For example "</param>
        /// <param name="escapeChar">In most cases it is \</param>
        /// <param name="missingDelimeterErrorMessage">{0} — First delimeter line id (row); {1} — First delimeter position (column)</param>
        public Tokenizer(
            char stringDelimiter, 
            char escapeChar, 
            string missingDelimeterErrorMessage)
        {
            StringDelimiter = stringDelimiter;
            EscapeChar = escapeChar;
            MissingDelimeterErrorMessageFormat = missingDelimeterErrorMessage ?? string.Empty;
        }

        public ResultWithValue<List<Token>> CreateTokens(
            int sourceLineId, int metaLineId, int firstTokenId, int firstMetaCharPos, string sourceLine)
        {
            List<Token> tokens = new List<Token>();
            foreach (var result in IterateTokensInLine(sourceLineId, metaLineId, firstTokenId, firstMetaCharPos, sourceLine))
            {
                if (!result.IsSuccessful)
                {
                    return new ResultWithValue<List<Token>>(false, result.ErrorMessage, tokens);
                }

                tokens.Add(result.Value);
            }

            return new ResultWithValue<List<Token>>(true, string.Empty, tokens);
        }

        public IEnumerable<ResultWithValue<Token>> IterateTokensInLine(
            int sourceLineId, int metaLineId, int firstTokenId, int firstMetaCharPos, string line)
        {
            if (line == null)
                yield break;

            int i = 0;
            int len = line.Length;

            int tokenId = firstTokenId;
            bool anyTokenYielded = false;
            Token? lastBuffered = null;

            while (true)
            {
                while (i < len && line[i] == ' ')
                    i++;

                if (i >= len)
                    break;

                int tokenStartPos = i;
                bool isInsideDelimiter = false;
                var sb = new StringBuilder();

                if (line[i] == StringDelimiter)
                {
                    isInsideDelimiter = true;
                    i++;
                    tokenStartPos = i;

                    bool closed = false;

                    while (i < len)
                    {
                        char c = line[i];

                        if (c == EscapeChar && i + 1 < len)
                        {
                            char next = line[i + 1];
                            if (next == StringDelimiter || next == EscapeChar)
                            {
                                sb.Append(next);
                                i += 2;
                                continue;
                            }
                        }

                        if (c == StringDelimiter)
                        {
                            i++;
                            closed = true;
                            break;
                        }

                        sb.Append(c);
                        i++;
                    }

                    if (!closed)
                    {
                        if (lastBuffered.HasValue)
                        {
                            var tPrev = lastBuffered.Value;
                            yield return new ResultWithValue<Token>(true, string.Empty, lastBuffered.Value);
                            lastBuffered = null;
                        }

                        string error = string.Format(MissingDelimeterErrorMessageFormat, sourceLineId + 1, tokenStartPos);
                        yield return new ResultWithValue<Token>(false, error, default);
                        yield break;
                    }
                }
                else
                {
                    while (i < len)
                    {
                        char c = line[i];
                        if (c == ' ' || c == StringDelimiter)
                            break;

                        sb.Append(c);
                        i++;
                    }
                }

                string value = sb.ToString();

                if (!isInsideDelimiter && value.Length == 0)
                    continue;

                Token token = new Token(
                    value: value,
                    isInsideDelimiter: isInsideDelimiter,
                    tokenId: tokenId,
                    metaLineId: metaLineId,
                    firstCharPositionInMetaLine: firstMetaCharPos + tokenStartPos,
                    sourceLineId: sourceLineId,
                    firstCharPositionInSourceLine: tokenStartPos,
                    isFirstInLine: !anyTokenYielded,
                    isLastInLine: false
                );
                tokenId++;

                if (lastBuffered.HasValue)
                {
                    yield return new ResultWithValue<Token>(true, string.Empty, lastBuffered.Value);
                }

                lastBuffered = token;
                anyTokenYielded = true;
            }

            if (lastBuffered.HasValue)
            {
                Token t = lastBuffered.Value;
                t.IsLastInLine = true;
                yield return new ResultWithValue<Token>(true, string.Empty, t);
            }
        }
    }
}
