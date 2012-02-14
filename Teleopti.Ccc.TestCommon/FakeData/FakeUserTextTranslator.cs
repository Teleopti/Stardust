using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Fake user translator that does not translate the text but return the original
    /// </summary>
    public class FakeUserTextTranslator : IUserTextTranslator
    {
        public string TranslateText(string textToTranslate)
        {
            return textToTranslate;
        }
    }
}
