using System.Globalization;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class PreferenceOpenNextMonthWorkflowControlSet : IUserSetup
	{
		public DateOnlyPeriod PreferencePeriod;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			PreferencePeriod = TestData.WorkflowControlSetPreferencesOpenNextMonth.PreferencePeriod;
			user.WorkflowControlSet = TestData.WorkflowControlSetPreferencesOpenNextMonth;
		}

	}
}