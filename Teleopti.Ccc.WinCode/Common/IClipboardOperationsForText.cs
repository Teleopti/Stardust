namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Uses the Clipboard object to perform clipborad operations with text data.
    /// </summary>
    public interface IClipboardOperationsForText
    {
        /// <summary>
        /// Copies the specified text into the Clipboard.
        /// </summary>
        /// <param name="text">The text to copy.</param>
        void Copy(string text);
    }
}
