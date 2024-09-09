using System.Text;

namespace TransactionsProcessor.Utilities
{
    public static class HelperMethods
    {
        public static string ToSnakeCaseAndUpper(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var stringBuilder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]) && i > 0)
                {
                    stringBuilder.Append('_');
                }
                stringBuilder.Append(char.ToUpper(input[i]));
            }
            return stringBuilder.ToString();
        }
    }
}
