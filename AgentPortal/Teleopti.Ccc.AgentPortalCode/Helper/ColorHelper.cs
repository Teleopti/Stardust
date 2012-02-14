using System.Drawing;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public static class ColorHelper
    {
        public static ColorDto CreateColorDto(Color color)
        {
            return new ColorDto
                       {
                           Alpha = color.A,
                           AlphaSpecified = true,
                           Blue = color.B,
                           BlueSpecified = true,
                           Green = color.G,
                           GreenSpecified = true,
                           Red = color.R,
                           RedSpecified = true
                       };
        }

        public static Color CreateColorFromDto(ColorDto colorDto)
        {
            return Color.FromArgb(colorDto.Alpha, colorDto.Red, colorDto.Green, colorDto.Blue);
        }
    }
}
