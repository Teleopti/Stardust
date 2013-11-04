﻿using System;
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

				var allowanceDay = allowanceCollection == null
										  ? null
										  : allowanceCollection.First(a => a.Item1 == dateOnly);

				var absenceTimeForDay = absenceTimeCollection == null
					                        ? 0
					                        : absenceTimeCollection.First(a => a.Date == dateOnly).AbsenceTime;

				var absenceHeadsForDay = absenceTimeCollection == null
											? 0
											: absenceTimeCollection.First(a => a.Date == dateOnly).HeadCounts;

				var fulltimeEquivalentForDay = allowanceDay == null
					                               ? 0
												   : allowanceDay.Item3.TotalMinutes;

				var allowanceMinutesForDay = allowanceDay == null
					                      ? 0
										  : allowanceDay.Item2.TotalMinutes;

				var allowanceForDay = allowanceDay == null
										  ? 0
										  : allowanceDay.Item4;

				var percent = 0d;
				var index = 0;
				//UseHeadCount
				if (allowanceDay != null && allowanceDay.Item6.Equals(true))
				{
					if (allowanceForDay > absenceHeadsForDay)
						percent = 100 * ( absenceHeadsForDay/ allowanceForDay);
					
					if (allowanceForDay - absenceHeadsForDay >= 1)
						index = 1;

					if (percent > 30 && allowanceForDay - absenceHeadsForDay >= 2)
						index = 2;
					
				}
				else
				{
					if (!Equals(allowanceMinutesForDay, .0))
						percent = 100 * ((allowanceMinutesForDay - absenceTimeForDay) / allowanceMinutesForDay);



					if (percent > 0 && (allowanceMinutesForDay - absenceTimeForDay) >= fulltimeEquivalentForDay)
						index = percent > 30 && (allowanceMinutesForDay - absenceTimeForDay) >= 2 * fulltimeEquivalentForDay ? 2 : 1;
				}
				

				ret.Add(new Tuple<DateOnly, string, string>(dateOnly, cssClass[index], texts[index]));
			}

			return ret;
		}
	}
}