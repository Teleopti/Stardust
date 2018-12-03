using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon
{
	public static class MultisiteDayExtensions
	{
		public static IMultisiteDay[] CreateMultisiteDays(this IMultisiteSkill skill, IScenario scenario, DateOnly dateOnly, ushort numberOfDays)
		{
			return Enumerable.Range(0, numberOfDays).Select(d =>
			{
				var date = dateOnly.AddDays(d);
				var dateOnlyPeriod = date.ToDateOnlyPeriod();

				var multisitePeriod = new MultisitePeriod(dateOnlyPeriod.ToDateTimePeriod(skill.TimeZone),
					skill.ChildSkills.ToDictionary(k => k, v => new Percent(1d / skill.ChildSkills.Count)));

				var multisitePeriods = new[] {multisitePeriod};

				var multisiteDay = new MultisiteDay(date, skill, scenario);
				multisiteDay.SetMultisitePeriodCollection(multisitePeriods);
				return multisiteDay;
			}).ToArray();
		}
	}
}