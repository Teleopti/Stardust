using System.Drawing;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class MaxSeatSkill : Skill
	{
		public MaxSeatSkill(ISite site, int intervalLength)
			:base(site.Description.Name, string.Empty, Color.DeepPink, intervalLength, new SkillTypePhone(new Description(), ForecastSource.MaxSeatSkill))
		{
			MaxSeats = site.MaxSeats.Value;
			SetId(site.Id.Value);
		}

		public int MaxSeats { get; }
	}
}