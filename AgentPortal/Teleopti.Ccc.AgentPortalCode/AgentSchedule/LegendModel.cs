using System.Drawing;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule
{
    public class LegendModel
    {
        public LegendModel(ActivityDto dto)
        {
            Color = ColorHelper.CreateColorFromDto(dto.DisplayColor);
            Name = dto.Description;
        }

        public LegendModel(AbsenceDto dto)
        {
            Color = ColorHelper.CreateColorFromDto(dto.DisplayColor);
            Name = dto.Name;
        }

        public Color Color { get; private set; }
        public string Name { get; private set; }
    }
}