using System.Globalization;

namespace SimpleContentLanguage
{
    public class BasicTypesParser<TResult> : IArgumentParser<TResult>
        where TResult : IParsable<TResult>
    {
        public readonly CultureInfo CultureInfo;
        public readonly string TypeName;
        public readonly string ParsingErrorMessageFormat;

        /// <param name="cultureInfo"></param>
        /// <param name="typeName"></param>
        /// <param name="errorMessageFormat">
        /// {0} — Parsing type name; {1} — Argument text; </param>
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
}
