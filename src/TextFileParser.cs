namespace SimpleContentLanguage
{
    public class TextFileParser : Parser
    {
        /// <inheritdoc/>
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

        public Result ParseTextFile(string relativeTextFilePath)
        {
            string path = Path.GetFullPath(relativeTextFilePath);
            using (StreamReader reader = new StreamReader(path))
            {
                return Parse(reader.ReadLine);
            }
        }
    }
}
