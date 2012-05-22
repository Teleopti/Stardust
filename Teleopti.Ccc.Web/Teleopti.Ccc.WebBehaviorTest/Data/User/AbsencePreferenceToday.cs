using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;
namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class AbsencePreferenceToday : BasePreference
	{
		public IAbsence Absence = TestData.AbsenceInContractTime;
		protected override PreferenceRestriction ApplyRestriction() { return new PreferenceRestriction { Absence = Absence }; }
		protected override DateTime ApplyDate(CultureInfo cultureInfo) { return DateTime.Today; }
	}
}
