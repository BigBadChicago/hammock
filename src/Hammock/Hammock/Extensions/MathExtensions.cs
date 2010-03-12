namespace Hammock.Extensions
{
    internal static class MathExtensions
    {
        public static int CountDecimalPlaces(this double input)
        {
            var value = input.ToString();

            var places = value.Substring(value.IndexOf('.') + 1).Length;

            return places;
        }
    }
}