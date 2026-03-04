namespace SimpleContentLanguage
{
    public struct Token
    {
        public string Value;
        public bool IsInsideDelimiter;
        public int TokenId;
        public int MetaLineId;
        public int FirstCharPositionInMetaLine;
        public int SourceLineId;
        public int FirstCharPositionInSourceLine;
        public bool IsFirstInLine;
        public bool IsLastInLine;

        public Token(
            string value,
            bool isInsideDelimiter,
            int tokenId,
            int metaLineId,
            int firstCharPositionInMetaLine,
            int sourceLineId,
            int firstCharPositionInSourceLine,
            bool isFirstInLine,
            bool isLastInLine)
        {
            Value = value;
            IsInsideDelimiter = isInsideDelimiter;
            TokenId = tokenId;
            MetaLineId = metaLineId;
            FirstCharPositionInMetaLine = firstCharPositionInMetaLine;
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
