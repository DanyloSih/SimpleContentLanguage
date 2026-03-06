namespace SimpleContentLanguage
{
    public abstract class ElementParser
    {
        public readonly IElementRecognizer ElementRecognizer;
        public readonly int RequiredArgumentsCount;

        protected readonly ErrorsConfig ErrorsConfig;

        protected ElementParser(
            IElementRecognizer elementRecognizer, 
            int requiredArgumentsCount,
            ErrorsConfig errorsConfig)
        {
            ElementRecognizer = elementRecognizer;
            RequiredArgumentsCount = requiredArgumentsCount;

            ErrorsConfig = errorsConfig;
        }

        public Result Parse(TokenizedBlock tokenizedBlock, TokenBounds elementBounds)
        {
            Token[] args = new Token[RequiredArgumentsCount];
            Token currentToken = elementBounds.StartToken;

            for (int i = 0; i < RequiredArgumentsCount; i++)
            {
                if (!tokenizedBlock.TryGetNextTokenInBounds(currentToken, elementBounds, out currentToken)
                 || ElementRecognizer.IsEnd(currentToken))
                {
                    return new Result(false, ErrorsConfig.GetIncorrectArgumentsCountError(
                        elementBounds.StartToken.SourceLineId + 1,
                        elementBounds.StartToken.FirstCharPositionInSourceLine,
                        elementBounds.StartToken.Text,
                        i,
                        RequiredArgumentsCount
                    ));
                }
                else
                {
                    args[i] = currentToken;
                }
            }

            return OnParse(args, tokenizedBlock, elementBounds);
        }

        protected abstract Result OnParse(Token[] args, TokenizedBlock tokenizedBlock, TokenBounds elementBounds);
    }
}
