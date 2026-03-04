namespace SimpleContentLanguage
{
    public interface IElementRecognizer
    {
        public bool IsStart(Token token);

        public bool IsEnd(Token token);
    }
}
