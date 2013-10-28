using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class AbsencePreferenceToday : BasePreference
	{
		public IAbsence Absence = TestData.AbsenceInContractTime;
		protected override PreferenceRestriction ApplyRestriction() { return new PreferenceRestriction { Absence = Absence }; }
		protected override DateTime ApplyDate(CultureInfo cultureInfo) { return DateOnlyForBehaviorTests.TestToday.Date; }
	}
}
