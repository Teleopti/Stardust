using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class PreferenceOpenNextMonthWorkflowControlSet : IUserSetup
	{
		public DateOnlyPeriod PreferencePeriod;

		public void Apply(IPerson user, CultureInfo cultureInfo)
		{
			PreferencePeriod = TestData.WorkflowControlSetPreferencesOpenNextMonth.PreferencePeriod;
			user.WorkflowControlSet = TestData.WorkflowControlSetPreferencesOpenNextMonth;
		}

	}
}