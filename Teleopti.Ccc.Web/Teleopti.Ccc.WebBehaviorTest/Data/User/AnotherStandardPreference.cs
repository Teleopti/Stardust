using System;
using System.Globalization;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class AnotherStandardPreference : AbsencePreference
	{
		protected override DateTime ApplyDate(CultureInfo cultureInfo) { return base.ApplyDate(cultureInfo).AddDays(1); }
	}
}