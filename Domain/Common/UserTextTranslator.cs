using System;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// A helper class for translating a user defined text to the localized language
    /// </summary>
    public class UserTextTranslator : IUserTextTranslator
    {
        /// <summary>
        /// Translates the string to the localized language.
        /// </summary>
        /// <param name="textToTranslate">The string to translate.</param>
        /// <returns>The translated string, or the original if no translation found.</returns>
        /// <remarks>
        /// Accepts a standard string, or a string which starts with two or three "x"-s. In the latter case
        /// it removes the starter "x"-s from the string in the first step. In the second it searches for a translation
        /// using the  <see cref="Resources.ResourceManager"/>. If no translation found it will return the original text.
        /// </remarks>
        public string TranslateText(string textToTranslate)
        {
            string result = textToTranslate;
            if (!String.IsNullOrEmpty(textToTranslate))
            {
                if (textToTranslate.StartsWith("xxx", StringComparison.OrdinalIgnoreCase)) textToTranslate = textToTranslate.Substring(3);
                if (textToTranslate.StartsWith("xx", StringComparison.OrdinalIgnoreCase)) textToTranslate = textToTranslate.Substring(2);

	            Resources.ResourceManager.IgnoreCase = true;
					string translatedText = Resources.ResourceManager.GetString(textToTranslate);
                if (!string.IsNullOrEmpty(translatedText))
                   result = translatedText;
            }
            return result;
        }
    }
}
