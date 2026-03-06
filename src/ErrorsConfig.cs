namespace SimpleContentLanguage
{
    public class ErrorsConfig
    {
        /// <summary>
        /// {0} - Line id, {1} - Char id
        /// </summary>
        public string MissingDelimeterErrorFormat
            = "({0}:{1}) Відкрито лапки, але до кінця рядка не знайдено відповідної закривальної лапки!";

        /// <summary>
        /// {0} - Line id, {1} - Char id, {2} — Start token text
        /// </summary>
        public string EndTokenAbsenceErrorFormat
            = "({0}:{1}) Для початкового токена \"{2}\", не знайдено відповідного закривального токена!";

        /// <summary>
        /// {0} - Line id, {1} - Char id, {2} — Start token text, {3} - Found args count, {4} - Expected args count
        /// </summary>
        public string IncorrectArgumentsCountErrorFormat
            = "({0}:{1}) Невірна кількість аргументів ({3}) для елемента \"{2}\". Очікується: {4}.";

        /// <summary>
        /// {0} - Line id, {1} - Char id, {2} — Expected type name, {3} — Parsing argument
        /// </summary>
        public string ArgumentParsingErrorFormat
            = "({0}:{1}) Невірний формат аргументу \"{3}\" для типу \"{2}\".";

        public string GetMissingDelimeterError(int lineId, int charId)
        {
            return string.Format(MissingDelimeterErrorFormat, lineId, charId);
        }

        public string GetEndTokenAbsenceError(int lineId, int charId, string startTokenText)
        {
            return string.Format(EndTokenAbsenceErrorFormat, lineId, charId, startTokenText);
        }

        public string GetIncorrectArgumentsCountError(
            int lineId, int charId, string startTokenText, int foundArgsCount, int expectedArgsCount)
        {
            return string.Format(
                IncorrectArgumentsCountErrorFormat, lineId, charId, startTokenText, foundArgsCount, expectedArgsCount);
        }

        public string GetArgumentParsingError(int lineId, int charId, string expectedType, string parsingArgument)
        {
            return string.Format(ArgumentParsingErrorFormat, lineId, charId, expectedType, parsingArgument);
        }
    }
}
