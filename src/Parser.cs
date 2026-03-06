using System.Text;

namespace SimpleContentLanguage
{
    public class Parser
    {
        public readonly Tokenizer Tokenizer;
        public readonly List<ElementParser> Parsers;

        private ParsingConfig _parsingConfig;
        private ErrorsConfig _errorsConfig;

        public Parser(
            Tokenizer tokenizer,
            List<ElementParser> parsers,
            ParsingConfig parsingConfig,
            ErrorsConfig errorsConfig)
        {
            Tokenizer = tokenizer;
            Parsers = parsers;
            _parsingConfig = parsingConfig;
            _errorsConfig = errorsConfig;
        }

        public Result Parse(Func<string?> getLineFunc)
        {
            List<TokenizedLine> lines = new();

            ElementParser? processingElement = null;
            Token startToken = default;
            int startTokenSourceLineId = 0;

            List<Token> tokens = new List<Token>();
            StringBuilder lineBuilder = new StringBuilder();

            string? line;
            int sourceLineCounter = -1;
            int metaLineCounter = 0;
            int previousTokensBatchCount = 0;
            while ((line = getLineFunc()) != null)
            {
                sourceLineCounter++;
                
                ResultWithValue<List<Token>> lineTokensResult = Tokenizer.CreateTokens(
                    sourceLineCounter, metaLineCounter, previousTokensBatchCount, lineBuilder.Length, line);

                if (!lineTokensResult.IsSuccessful)
                {
                    return new Result(false, lineTokensResult.ErrorMessage);
                }

                tokens.AddRange(lineTokensResult.Value ?? new List<Token>());

                if (tokens.Count == 0 || !tokens[^1].Text.Equals(_parsingConfig.LineContinuationToken))
                {
                    lineBuilder.Append(line);
                    if (processingElement != null)
                    {
                        lines.Add(new TokenizedLine(lineBuilder.ToString(), metaLineCounter++, tokens));
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
                        foreach (ElementParser parser in Parsers)
                        {
                            if (parser.ElementRecognizer.IsStart(token))
                            {
                                processingElement = parser;
                                startToken = token;
                                startTokenSourceLineId = sourceLineCounter;
                                lines.Add(new TokenizedLine(lineBuilder.ToString(), metaLineCounter++, tokens));
                            }
                        }
                    }

                    if (processingElement != null)
                    {
                        if (processingElement.ElementRecognizer.IsEnd(token))
                        {
                            Result parsingResult = processingElement.Parse(
                                new TokenizedBlock(lines), new TokenBounds(startToken, token));

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
                string error = _errorsConfig.GetEndTokenAbsenceError(
                    startTokenSourceLineId + 1,
                    startToken.FirstCharPositionInSourceLine,
                    startToken.Text!);

                return new Result(true, error);
            }

            return new Result(true, string.Empty);
        }
    }
}
