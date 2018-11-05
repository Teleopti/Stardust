﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AbsenceRequestProbabilityProvider : IAbsenceRequestProbabilityProvider
	{
		private readonly IAllowanceProvider _allowanceProvider;
		private readonly IAbsenceTimeProvider _absenceTimeProvider;
		private readonly INow _now;

		public AbsenceRequestProbabilityProvider(
			IAllowanceProvider allowanceProvider,
			IAbsenceTimeProvider absenceTimeProvider,
			INow now)
		{
			_allowanceProvider = allowanceProvider;
			_absenceTimeProvider = absenceTimeProvider;
			_now = now;
		}

		public List<IAbsenceRequestProbability> GetAbsenceRequestProbabilityForPeriod(DateOnlyPeriod period)
		{
			var absenceTimeCollection = _absenceTimeProvider.GetAbsenceTimeForPeriod(period).ToList();
			var allowanceCollection = _allowanceProvider.GetAllowanceForPeriod(period).ToList();

			var ret = new List<IAbsenceRequestProbability>();

			foreach (var dateOnly in period.DayCollection())
			{
				var absenceTimeForDay = .0;
				var absenceHeadsForDay = .0;
				if (absenceTimeCollection.Any())
				{
					absenceTimeForDay = absenceTimeCollection.First(a => a.Date == dateOnly.Date).AbsenceTime;
					absenceHeadsForDay = absenceTimeCollection.First(a => a.Date == dateOnly.Date).HeadCounts;
				}

				var allowanceDay = allowanceCollection.Any()
					? allowanceCollection.First(a => a.Date == dateOnly)
					: null;

				var fulltimeEquivalentForDay = .0;
				var allowanceMinutesForDay = .0;
				var allowanceForDay = .0;
				if (allowanceDay != null)
				{
					fulltimeEquivalentForDay = allowanceDay.Heads.TotalMinutes;
					allowanceMinutesForDay = allowanceDay.Time.TotalMinutes;
					allowanceForDay = allowanceDay.AllowanceHeads;
				}

				var probabilityIndex = -1;
				if (allowanceDay != null && allowanceDay.ValidateBudgetGroup)
				{
					probabilityIndex = allowanceDay.UseHeadCount
						? getAllowanceIndexWithHeadCount(allowanceForDay, absenceHeadsForDay)
						: getAllowanceIndex(allowanceMinutesForDay, absenceTimeForDay, fulltimeEquivalentForDay);

					if (dateOnly < _now.ServerDate_DontUse())
					{
						probabilityIndex = 0;
					}
				}

				ret.Add(new AbsenceRequestProbability
				{
					Date = dateOnly,
					CssClass = getBudgetCssClass(probabilityIndex),
					Text = getAbsenceProbabilityText(probabilityIndex),
					Availability = allowanceDay != null && allowanceDay.Availability
				});
			}

			return ret;
		}

		private static int getAllowanceIndex(double allowanceMinutesForDay, double absenceTimeForDay,
			double fulltimeEquivalentForDay)
		{
			var probabilityIndex = 0;
			var percent = .0;
			var timeDiff = allowanceMinutesForDay - absenceTimeForDay;
			if (!Equals(allowanceMinutesForDay, .0))
			{
				percent = 100*(timeDiff/allowanceMinutesForDay);
			}

			if (percent > 0 && timeDiff >= fulltimeEquivalentForDay)
			{
				probabilityIndex = percent > 30 && timeDiff >= 2*fulltimeEquivalentForDay ? 2 : 1;
			}
			return probabilityIndex;
		}

		private static int getAllowanceIndexWithHeadCount(double allowanceForDay, double absenceHeadsForDay)
		{
			var probabilityIndex = 0;
			var percent = .0;
			var allowanceDiff = allowanceForDay - absenceHeadsForDay;

			if (allowanceDiff > 0)
			{
				percent = 100 - (100*(absenceHeadsForDay/allowanceForDay));
			}

			if (allowanceDiff >= 1)
			{
				probabilityIndex = 1;
			}

			if (percent > 30 && allowanceDiff >= 2)
			{
				probabilityIndex = 2;
			}
			return probabilityIndex;
		}

		private static string getBudgetCssClass(int probabilityIndex)
		{
			switch (probabilityIndex)
			{
				case 0: return BudgetCssClass.Poor;
				case 1: return BudgetCssClass.Fair;
				case 2: return BudgetCssClass.Good;
				default: return string.Empty;
			}
		}

		private static string getAbsenceProbabilityText(int probabilityIndex)
		{
			switch (probabilityIndex)
			{
				case 0: return UserTexts.Resources.Poor;
				case 1: return UserTexts.Resources.Fair;
				case 2: return UserTexts.Resources.Good;
				default: return string.Empty;
			}
		}
	}

	public static class BudgetCssClass
	{
		internal const string Poor = "poor";
		internal const string Fair = "fair";
		internal const string Good = "good";
	}
}