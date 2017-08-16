using System.Drawing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public static class ConfidentialPayloadValues
    {
        public readonly static Description Description = new Description(Resources.ConfidentPayloadName, Resources.ConfidentPayloadShortName);
        public readonly static Color DisplayColor = Color.Gray;
		public readonly static string DisplayColorHex = "#808080";

	    public static Description TranslatedDescription(IUserTextTranslator userTextTranslator)
	    {
		    return new Description(userTextTranslator.TranslateText(Resources.ConfidentPayloadName),
			    userTextTranslator.TranslateText(Resources.ConfidentPayloadShortName));
	    }
    }
}
