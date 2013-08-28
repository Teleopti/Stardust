using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class AbsencePreferenceInContractTimeOnWeekday : BasePreference
	{
		public int Weekday;
		public IAbsence Absence = TestData.AbsenceInContractTime;

		public AbsencePreferenceInContractTimeOnWeekday(int weekday)
		{
			Weekday = weekday;
		}

		protected override PreferenceRestriction ApplyRestriction()
		{
			return new PreferenceRestriction { Absence = Absence };
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
            return DateHelper.GetFirstDateInWeek(DateOnlyForBehaviorTests.TestToday.Date, cultureInfo).AddDays(Weekday - 1);
		}
	}
}