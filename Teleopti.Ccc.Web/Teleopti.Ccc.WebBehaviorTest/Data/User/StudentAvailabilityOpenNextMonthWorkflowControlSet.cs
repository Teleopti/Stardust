using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
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