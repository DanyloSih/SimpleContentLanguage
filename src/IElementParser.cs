namespace SimpleContentLanguage
{
    public interface IElementParser
    {
        public IElementRecognizer ElementRecognizer { get; }

        public Result Parse(List<TokenizedLine> tokenizedLines, ElementBounds elementBounds);
    }
}
