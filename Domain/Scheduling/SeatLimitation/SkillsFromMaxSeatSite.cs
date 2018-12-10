using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class SkillsFromMaxSeatSite
	{
		private readonly ITimeZoneGuard _timeZoneGuard;

		public SkillsFromMaxSeatSite(ITimeZoneGuard timeZoneGuard)
		{
			_timeZoneGuard = timeZoneGuard;
		}

		//mainly copied from old CreateSkillsFromMaxSeatSites
		public MaxSeatSkill CreateMaxSeatSkill(ISite site, int intervalLength)
		{
			if (!site.MaxSeats.HasValue)
				return null;

			var newSkill = new MaxSeatSkill(site, intervalLength);

			var templateSkillDataPeriods = new List<ITemplateSkillDataPeriod>();
			var baseDate = SkillDayTemplate.BaseDate.Date;
			newSkill.TimeZone = _timeZoneGuard.CurrentTimeZone();
			var numberOfIntervals = 24 * 60 / intervalLength;
			for (var i = 0; i < numberOfIntervals; i++)
			{
				var period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(baseDate.AddMinutes(intervalLength * i), baseDate.AddMinutes(intervalLength * (i + 1)), newSkill.TimeZone);

				var templateSkillDataPeriod = new TemplateSkillDataPeriod(new ServiceAgreement(), new SkillPersonData(0, 0), period);
				templateSkillDataPeriods.Add(templateSkillDataPeriod);
			}

			newSkill.SetTemplateAt(0, new SkillDayTemplate("fake", templateSkillDataPeriods));
			newSkill.SetTemplateAt(1, new SkillDayTemplate("fake", templateSkillDataPeriods));
			newSkill.SetTemplateAt(2, new SkillDayTemplate("fake", templateSkillDataPeriods));
			newSkill.SetTemplateAt(3, new SkillDayTemplate("fake", templateSkillDataPeriods));
			newSkill.SetTemplateAt(4, new SkillDayTemplate("fake", templateSkillDataPeriods));
			newSkill.SetTemplateAt(5, new SkillDayTemplate("fake", templateSkillDataPeriods));
			newSkill.SetTemplateAt(6, new SkillDayTemplate("fake", templateSkillDataPeriods));

			return newSkill;
		}
	}
}