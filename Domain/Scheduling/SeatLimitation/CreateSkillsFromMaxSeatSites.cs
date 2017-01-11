using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public interface ICreateSkillsFromMaxSeatSites
	{
		IEnumerable<ISkill> CreateSkillList(IEnumerable<ISite> sites, int intervalLength);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public class CreateSkillsFromMaxSeatSites : ICreateSkillsFromMaxSeatSites
	{
		private readonly IUserTimeZone _userTimeZone;

		public CreateSkillsFromMaxSeatSites(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}

        public IEnumerable<ISkill> CreateSkillList(IEnumerable<ISite> sites, int intervalLength)
		{
			IList<ISkill> newSkills = new List<ISkill>();
			foreach (var site in sites)
			{
				if (!site.MaxSeats.HasValue)
					continue;

				var newSkill = new MaxSeatSkill(site, intervalLength);
				
				IList<ITemplateSkillDataPeriod> templateSkillDataPeriods = new List<ITemplateSkillDataPeriod>();
                var baseDate = SkillDayTemplate.BaseDate.Date;
			    var timeZone = _userTimeZone.TimeZone();
			    newSkill.TimeZone = timeZone;
				var numberOfIntervals = 24*60/intervalLength;
				for (int i = 0; i < numberOfIntervals; i++)
                {
					DateTimePeriod period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(baseDate.AddMinutes(intervalLength * i),
																		 baseDate.AddMinutes(intervalLength * (i + 1)), timeZone);

					ITemplateSkillDataPeriod templateSkillDataPeriod = new TemplateSkillDataPeriod(new ServiceAgreement(),
				                                                                               new SkillPersonData(0, 0), period);
					templateSkillDataPeriod.MaxSeats = site.MaxSeats.Value;
					templateSkillDataPeriods.Add(templateSkillDataPeriod);
				}

				newSkill.SetTemplateAt(0, new SkillDayTemplate("fake", templateSkillDataPeriods));
				newSkill.SetTemplateAt(1, new SkillDayTemplate("fake", templateSkillDataPeriods));
				newSkill.SetTemplateAt(2, new SkillDayTemplate("fake", templateSkillDataPeriods));
				newSkill.SetTemplateAt(3, new SkillDayTemplate("fake", templateSkillDataPeriods));
				newSkill.SetTemplateAt(4, new SkillDayTemplate("fake", templateSkillDataPeriods));
				newSkill.SetTemplateAt(5, new SkillDayTemplate("fake", templateSkillDataPeriods));
				newSkill.SetTemplateAt(6, new SkillDayTemplate("fake", templateSkillDataPeriods));
				newSkills.Add(newSkill);
				site.MaxSeatSkill = newSkill;
			}
	        return newSkills;
		}
	}
}