using System.Drawing;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public static class ConfidentialPayloadValues
    {
        public readonly static Description Description = new Description(Resources.ConfidentPayloadName, Resources.ConfidentPayloadShortName);
        public readonly static Color DisplayColor = Color.Gray;
		public readonly static string DisplayColorHex = "#808080";
    }
}
