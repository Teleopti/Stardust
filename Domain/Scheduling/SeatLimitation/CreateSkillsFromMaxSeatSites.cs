using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public interface ICreateSkillsFromMaxSeatSites
	{
		IEnumerable<ISkill> CreateSkillList(IEnumerable<ISite> sites, int intervalLength);
	}

	public class CreateSkillsFromMaxSeatSites : ICreateSkillsFromMaxSeatSites
	{
        public IEnumerable<ISkill> CreateSkillList(IEnumerable<ISite> sites, int intervalLength)
		{
			IList<ISkill> newSkills = new List<ISkill>();
			foreach (var site in sites)
			{
				if (!site.MaxSeats.HasValue)
					continue;

				ISkill newSkill = new Skill(site.Description.Name, "", Color.DeepPink, intervalLength,
				                            new SkillTypePhone(new Description(), ForecastSource.MaxSeatSkill));

                newSkill.SetId(site.Id);

				IList<ITemplateSkillDataPeriod> templateSkillDataPeriods = new List<ITemplateSkillDataPeriod>();
                var baseDate = SkillDayTemplate.BaseDate.Date;
			    var timeZone = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;
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