using System.Globalization;

namespace SimpleContentLanguage
{
    public class BasicTypesParser<TResult> : IArgumentParser<TResult>
        where TResult : IParsable<TResult>
    {
        public readonly CultureInfo CultureInfo;
        public readonly string TypeName;

        private readonly ErrorsConfig _errorsConfig;

        public BasicTypesParser(
            CultureInfo cultureInfo, 
            string typeName, 
            ErrorsConfig errorsConfig)
        {
            CultureInfo = cultureInfo;
            TypeName = typeName;
            _errorsConfig = errorsConfig;
        }

        public ResultWithValue<TResult> Parse(Token argumentToken)
        {
            bool isSuccessful = TResult.TryParse(argumentToken.Text, CultureInfo, out TResult? result);
            string errorMessage = string.Empty;

            if (!isSuccessful)
            {
                errorMessage = _errorsConfig.GetArgumentParsingError(
                    argumentToken.SourceLineId + 1, 
                    argumentToken.FirstCharPositionInSourceLine,
                    TypeName,
                    argumentToken.Text);
            }

            return new ResultWithValue<TResult>(isSuccessful, errorMessage, result);
        }
    }
}
