namespace Teleopti.Ccc.WinCode.Converters
{

    public abstract class ParseBehavior<T>:IParseBehavior<T>
    {
        public abstract bool ConvertFromString(string value, out T returnValue);

        public abstract string ConvertToString(T value);
    }
}
