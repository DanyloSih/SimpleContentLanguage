
namespace SimpleContentLanguage
{
    public class TextFileParser : Parser
    {
        public TextFileParser(
            Tokenizer tokenizer, 
            List<ElementParser> parsers, 
            ParsingConfig parsingConfig, 
            ErrorsConfig errorsConfig) 
            : base(tokenizer, parsers, parsingConfig, errorsConfig) { }

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
