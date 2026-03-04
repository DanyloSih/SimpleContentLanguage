using System.Text;

namespace SimpleContentLanguage
{
    public class Parser
    {
        public readonly Tokenizer Tokenizer;
        public readonly List<IElementParser> Parsers;
        public readonly string LineContinuationToken;
        public readonly string EndTokenAbsenceErrorMessageFormat;

        /// <param name="tokenizer"></param>
        /// <param name="parsers"></param>
        /// <param name="lineContinuationToken"></param>
        /// <param name="endTokenAbsenceErrorMessageFormat">
        /// {0} — Start token text; {1} — Start token line id (row); {2} — Start token position (column)</param>
        public Parser(
            Tokenizer tokenizer,
            List<IElementParser> parsers,
            string lineContinuationToken,
            string endTokenAbsenceErrorMessageFormat)
        {
            Tokenizer = tokenizer;
            Parsers = parsers;
            LineContinuationToken = lineContinuationToken;
            EndTokenAbsenceErrorMessageFormat = endTokenAbsenceErrorMessageFormat;
        }

        public Result Parse(Func<string?> getLineFunc)
        {
            List<TokenizedLine> lines = new();

            IElementParser? processingElement = null;
            Token startToken = default;
            int startTokenRealLineId = 0;

            List<Token> tokens = new List<Token>();
            StringBuilder lineBuilder = new StringBuilder();

            string? line;
            int realLineCounter = -1;
            int previousTokensBatchCount = 0;
            while ((line = getLineFunc()) != null)
            {
                realLineCounter++;
                
                ResultWithValue<List<Token>> lineTokensResult = Tokenizer.CreateTokens(
                    realLineCounter, previousTokensBatchCount, line);

                if (!lineTokensResult.IsSuccessful)
                {
                    return new Result(false, lineTokensResult.ErrorMessage);
                }

                tokens.AddRange(lineTokensResult.Value ?? new List<Token>());

                if (tokens.Count == 0 || !tokens[^1].Value.Equals(LineContinuationToken))
                {
                    lineBuilder.Append(line);
                    if (processingElement != null)
                    {
                        lines.Add(new TokenizedLine(lineBuilder.ToString(), tokens));
                    }
                    previousTokensBatchCount = 0;
                }
                else
                {
                    Token lastToken = tokens[^1];
                    tokens.RemoveAt(tokens.Count - 1);
                    previousTokensBatchCount = tokens.Count;
                    lineBuilder.Append(lastToken.RemoveFromLine(line));
                    continue;
                }

                foreach (Token token in tokens)
                {
                    if (processingElement == null)
                    {
                        foreach (IElementParser parser in Parsers)
                        {
                            if (parser.ElementRecognizer.IsStart(token))
                            {
                                processingElement = parser;
                                startToken = token;
                                startTokenRealLineId = realLineCounter;
                                lines.Add(new TokenizedLine(lineBuilder.ToString(), tokens));
                            }
                        }
                    }

                    if (processingElement != null)
                    {
                        if (processingElement.ElementRecognizer.IsEnd(token))
                        {
                            Result parsingResult = processingElement.Parse(lines, new ElementBounds(startToken, token));
                            if (!parsingResult.IsSuccessful)
                            {
                                return new Result(false, parsingResult.ErrorMessage);
                            }
                            processingElement = null;
                            lines = new List<TokenizedLine>();
                        }
                    }
                };

                lineBuilder = new StringBuilder();
                tokens = new List<Token>();
            }

            if (processingElement != null)
            {
                string error = string.Format(
                    EndTokenAbsenceErrorMessageFormat,
                    startToken.Value,
                    startTokenRealLineId + 1,
                    startToken.FirstCharPositionInSourceLine);

                return new Result(true, error);
            }

            return new Result(true, string.Empty);
        }
    }
}
