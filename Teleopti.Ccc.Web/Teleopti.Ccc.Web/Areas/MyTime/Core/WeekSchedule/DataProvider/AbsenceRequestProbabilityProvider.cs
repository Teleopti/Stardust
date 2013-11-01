using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public interface IAbsenceRequestProbabilityProvider
	{
		List<Tuple<DateOnly, string, string>> GetAbsenceRequestProbabilityForPeriod(DateOnlyPeriod period);
	}
	public class AbsenceRequestProbabilityProvider : IAbsenceRequestProbabilityProvider
	{
		private readonly IAllowanceProvider _allowanceProvider;
		private readonly IAbsenceTimeProvider _absenceTimeProvider;
		readonly string[] texts = {UserTexts.Resources.Poor, UserTexts.Resources.Fair, UserTexts.Resources.Good};
		readonly string[] cssClass = {"red", "yellow", "green"};

		public AbsenceRequestProbabilityProvider(IAllowanceProvider allowanceProvider,
		                                         IAbsenceTimeProvider absenceTimeProvider)
		{
			_allowanceProvider = allowanceProvider;
			_absenceTimeProvider = absenceTimeProvider;
		}

		public List<Tuple<DateOnly, string, string>> GetAbsenceRequestProbabilityForPeriod(DateOnlyPeriod period)
		{
			var absenceTimeCollection = _absenceTimeProvider.GetAbsenceTimeForPeriod(period);
			var allowanceCollection = _allowanceProvider.GetAllowanceForPeriod(period);

			var ret = new List<Tuple<DateOnly, string, string>>();

			foreach (var dateOnly in period.DayCollection())
			{

				var absenceTimeForDay = absenceTimeCollection == null
					                        ? 0
					                        : absenceTimeCollection.First(a => a.Date == dateOnly).AbsenceTime;

				var fulltimeEquivalentForDay = allowanceCollection == null
					                               ? 0
					                               : allowanceCollection.First(a => a.Item1 == dateOnly).Item3.TotalMinutes;

				var allowanceForDay = allowanceCollection == null
					                      ? 0
					                      : allowanceCollection.First(a => a.Item1 == dateOnly).Item2.TotalMinutes;


				var percent = 0d;
				if (!Equals(allowanceForDay, .0))
					percent = 100*((allowanceForDay - absenceTimeForDay)/allowanceForDay);


				var index = 0;
				if (percent > 0 && (allowanceForDay - absenceTimeForDay) >= fulltimeEquivalentForDay)
					index = percent > 30 && (allowanceForDay - absenceTimeForDay) >= 2*fulltimeEquivalentForDay ? 2 : 1;

				ret.Add(new Tuple<DateOnly, string, string>(dateOnly, cssClass[index], texts[index]));
			}

			return ret;
		}
	}
}