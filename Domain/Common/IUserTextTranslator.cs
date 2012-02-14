using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Common
{
    public interface IUserTextTranslator
    {
        /// <summary>
        /// Translates the text to the localized language.
        /// </summary>
        /// <param name="textToTranslate">The text to translate.</param>
        /// <returns>The translated string, or the original if no translation found.</returns>
        /// <remarks>
        /// Accepts a standard string, or a string which starts with two or three "x"-s. In the latter case
        /// it removes the starter "x"-s from the string in the first step. In the second it searches for a translation
        /// using the  <see cref="Resources.ResourceManager"/>. If no translation found it will return the original text.
        /// </remarks>
        string TranslateText(string textToTranslate);
    }
}