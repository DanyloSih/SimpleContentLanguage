namespace SimpleContentLanguage
{
    public interface IElementParser
    {
        public IElementRecognizer ElementRecognizer { get; }

        public Result Parse(TokenizedBlock tokenizedBlock, TokenBounds elementBounds);
    }
}
