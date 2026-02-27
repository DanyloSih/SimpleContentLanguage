using System.Globalization;
using System.Text;

namespace SimpleContentLanguage
{
    public struct Token
    {
        public readonly string Value;
        public readonly bool IsInsideDelimiter;
        public readonly int TokenId;
        public readonly int FirstCharPositionInLine;
        public readonly bool IsFirstInLine;
        public readonly bool IsLastInLine;

        public Token(
            string value,
            bool isInsideDelimiter,
            int tokenId,
            int firstCharPositionInLine,
            bool isFirstInLine,
            bool isLastInLine)
        {
            Value = value;
            IsInsideDelimiter = isInsideDelimiter;
            TokenId = tokenId;
            FirstCharPositionInLine = firstCharPositionInLine;
            IsFirstInLine = isFirstInLine;
            IsLastInLine = isLastInLine;
        }
    }

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

    public class Result
    {
        public readonly bool IsSuccessful;
        public readonly string ErrorMessage;

        public Result(bool isSuccessful, string errorMessage)
        {
            IsSuccessful = isSuccessful;
            ErrorMessage = errorMessage;
        }
    }

    public class ResultWithValue<TValue> : Result
    {
        public readonly TValue? Value;

        public ResultWithValue(
            bool isSuccessful,
            string errorMessage,
            TValue? value) : base(isSuccessful, errorMessage)
        {
            Value = value;
        }
    }

    public class TokenizedLine
    {
        public readonly int LineId;
        public readonly string OriginalLine;
        public readonly List<Token> Tokens;

        public TokenizedLine(int lineId, string originalLine, List<Token> tokens)
        {
            LineId = lineId;
            OriginalLine = originalLine;
            Tokens = tokens;
        }
    }

    public interface IArgumentParser<TResult>
    {
        public ResultWithValue<TResult> Parse(string argumentText);
    }

    public interface IElementRecognizer
    {
        public bool IsStart(Token token);

        public bool IsEnd(Token token);
    }

    public interface IElementParser
    {
        public IElementRecognizer ElementRecognizer { get; }

        public Result Parse(List<TokenizedLine> tokenizedLines, ElementBounds elementBounds);

        public string GetUsageInfoText();
    }

    public class BasicTypesParser<TResult> : IArgumentParser<TResult>
        where TResult : IParsable<TResult>
    {
        public readonly CultureInfo CultureInfo;
        public readonly string TypeName;

        /// <summary>
        /// Error message format:
        /// <list type="bullet">
        /// <item>{0} — <see cref="TypeName"/></item>
        /// <item>{1} — ArgumentText</item>
        /// </list>
        /// </summary>
        public readonly string ParsingErrorMessageFormat;

        public BasicTypesParser(
            CultureInfo cultureInfo, 
            string typeName, 
            string errorMessageFormat)
        {
            CultureInfo = cultureInfo;
            TypeName = typeName;
            ParsingErrorMessageFormat = errorMessageFormat;
        }

        public ResultWithValue<TResult> Parse(string argumentText)
        {
            bool isSuccessful = TResult.TryParse(argumentText, CultureInfo, out TResult? result);
            string errorMessage = string.Empty;

            if (!isSuccessful)
            {
                errorMessage = string.Format(ParsingErrorMessageFormat, TypeName, argumentText);
            }

            return new ResultWithValue<TResult>(isSuccessful, errorMessage, result);
        }
    }

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

    public abstract class BlockRecognizer<TResult> : IElementRecognizer
    {
        public readonly string StartToken;
        public readonly string EndToken;

        protected BlockRecognizer(string startToken, string endToken)
        {
            StartToken = startToken;
            EndToken = endToken;
        }

        public virtual bool IsStart(Token token)
        {
            return token.Value.Equals(StartToken);
        }

        public virtual bool IsEnd(Token token)
        {
            return token.Value.Equals(EndToken);
        }
    }

    public class Tokenizer
    {
        public readonly char StringDelimiter;
        public readonly char EscapeChar;
        /// <summary>
        /// Error message format:
        /// <list type="bullet">
        /// <item>{0} — LineId</item>
        /// <item>{1} — FirstDelimeterPosition</item>
        /// </list>
        /// </summary>
        public readonly string MissingDelimeterErrorMessageFormat;

        public Tokenizer(char stringDelimiter, string missingDelimeterErrorMessage, char escapeChar = '\\')
        {
            StringDelimiter = stringDelimiter;
            EscapeChar = escapeChar;
            MissingDelimeterErrorMessageFormat = missingDelimeterErrorMessage ?? string.Empty;
        }

        public ResultWithValue<List<Token>> CreateTokens(int lineId, string line)
        {
            List<Token> tokens = new List<Token>();
            foreach (var result in IterateTokensInLine(lineId, line))
            {
                if (!result.IsSuccessful)
                {
                    return new ResultWithValue<List<Token>>(false, result.ErrorMessage, tokens);
                }

                tokens.Add(result.Value);
            }

            return new ResultWithValue<List<Token>>(true, string.Empty, tokens);
        }

        public IEnumerable<ResultWithValue<Token>> IterateTokensInLine(int lineId, string line)
        {
            if (line == null)
                yield break;

            int i = 0;
            int len = line.Length;

            int tokenId = 0;
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

                        string error = string.Format(MissingDelimeterErrorMessageFormat, lineId, tokenStartPos);
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
                    value,
                    isInsideDelimiter,
                    tokenId,
                    tokenStartPos,
                    !anyTokenYielded,
                    false
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
                yield return new ResultWithValue<Token>(true, string.Empty, new Token(
                    value: t.Value,
                    isInsideDelimiter: t.IsInsideDelimiter,
                    tokenId: t.TokenId,
                    firstCharPositionInLine: t.FirstCharPositionInLine,
                    isFirstInLine: t.IsFirstInLine,
                    isLastInLine: true
                ));
            }
        }
    }

    public class Parser
    {
        public readonly Tokenizer Tokenizer;
        public readonly List<IElementParser> Parsers;
        public readonly string LineContinuationToken;
        /// <summary>
        /// Error message format:
        /// <list type="bullet">
        /// <item>{0} — Start Token</item>
        /// <item>{1} — Start Token Row</item>
        /// <item>{2} — Start Token Column</item>
        /// </list>
        /// </summary>
        public readonly string EndTokenAbsenceErrorMessageFormat;

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
            int startTokenLineId = 0;

            List<Token> tokens = new List<Token>();
            StringBuilder lineBuilder = new StringBuilder();

            string? line;
            int lineCounter = 0;
            while ((line = getLineFunc()) != null)
            {
                lineBuilder.AppendLine(line);
                ResultWithValue<List<Token>> lineTokensResult = Tokenizer.CreateTokens(lineCounter, line);
                if (!lineTokensResult.IsSuccessful)
                {
                    return new Result(false, lineTokensResult.ErrorMessage);
                }

                tokens.AddRange(lineTokensResult.Value ?? new List<Token>());

                if (tokens.Count == 0 || !tokens[^1].Value.Equals(LineContinuationToken))
                {
                    lines.Add(new TokenizedLine(lineCounter, lineBuilder.ToString(), tokens));
                }
                else
                {
                    Token lastToken = tokens[^1];
                    tokens[^1] = new Token(
                        lastToken.Value,
                        lastToken.IsInsideDelimiter,
                        lastToken.TokenId,
                        lastToken.FirstCharPositionInLine,
                        lastToken.IsFirstInLine,
                        false);
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
                                startTokenLineId = lineCounter;
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

                            if (!token.IsLastInLine)
                            {
                                lines.Add(new TokenizedLine(lineCounter, line, lineTokensResult.Value!));
                            }
                        }
                    }
                };

                lineBuilder.Clear();
                tokens.Clear();
                lineCounter++;
            }

            if (processingElement != null)
            {
                string error = string.Format(
                    EndTokenAbsenceErrorMessageFormat,
                    startToken,
                    startTokenLineId,
                    startToken.FirstCharPositionInLine);

                return new Result(true, error);
            }

            return new Result(true, string.Empty);
        }
    }

    public class TextFileParser : Parser
    {
        public TextFileParser(
            Tokenizer tokenizer,
            List<IElementParser> parsers,
            string lineContinuationToken,
            string endTokenAbsenceErrorMessageFormat) 
        : base(
            tokenizer,
            parsers,
            lineContinuationToken,
            endTokenAbsenceErrorMessageFormat) { }

        public void ParseTextFile(string relativeTextFilePath)
        {
            string path = Path.GetFullPath(relativeTextFilePath);
            using (StreamReader reader = new StreamReader(path))
            {
                Parse(reader.ReadLine);
            }
        }
    }
}
