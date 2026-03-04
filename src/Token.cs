namespace SimpleContentLanguage
{
    public struct Token
    {
        public readonly string Value;
        public readonly bool IsInsideDelimiter;
        public readonly int TokenId;
        public readonly int SourceLineId;
        public readonly int FirstCharPositionInSourceLine;
        public readonly bool IsFirstInLine;
        public readonly bool IsLastInLine;

        public Token(
            string value,
            bool isInsideDelimiter,
            int tokenId,
            int sourceLineId,
            int firstCharPositionInSourceLine,
            bool isFirstInLine,
            bool isLastInLine)
        {
            Value = value;
            IsInsideDelimiter = isInsideDelimiter;
            TokenId = tokenId;
            SourceLineId = sourceLineId;
            FirstCharPositionInSourceLine = firstCharPositionInSourceLine;
            IsFirstInLine = isFirstInLine;
            IsLastInLine = isLastInLine;
        }

        public string RemoveFromLine(string sourceLine)
        {
            if (string.IsNullOrEmpty(sourceLine))
            {
                return sourceLine;
            }

            if (FirstCharPositionInSourceLine < 0 
             || FirstCharPositionInSourceLine >= sourceLine.Length)
            {
                return sourceLine;
            }

            int length = Value?.Length ?? 0;

            if (FirstCharPositionInSourceLine + length > sourceLine.Length)
            {
                length = sourceLine.Length - FirstCharPositionInSourceLine;
            }

            return sourceLine.Remove(FirstCharPositionInSourceLine, length);
        }
    }
}
