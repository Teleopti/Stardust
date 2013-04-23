using System.Drawing;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public static class ColorHelper
    {
        public static ColorDto CreateColorDto(Color color)
        {
            return new ColorDto(color);
        }

        public static Color CreateColorFromDto(ColorDto colorDto)
        {
            return Color.FromArgb(colorDto.Alpha, colorDto.Red, colorDto.Green, colorDto.Blue);
        }
    }
}
