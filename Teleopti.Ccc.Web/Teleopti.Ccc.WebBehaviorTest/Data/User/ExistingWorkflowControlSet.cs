using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class ExistingWorkflowControlSet : IUserSetup
	{
		public DateOnlyPeriod StudentAvailabilityPeriod;
		public DateOnlyPeriod StudentAvailabilityInputPeriod;
		public DateOnlyPeriod PreferencePeriod;
		public DateOnlyPeriod PreferenceInputPeriod;

		public void Apply(IPerson user, CultureInfo cultureInfo)
		{
			StudentAvailabilityPeriod = TestData.WorkflowControlSetExisting.StudentAvailabilityPeriod;
			StudentAvailabilityInputPeriod = TestData.WorkflowControlSetExisting.StudentAvailabilityInputPeriod;
			PreferencePeriod = TestData.WorkflowControlSetExisting.PreferencePeriod;
			PreferenceInputPeriod = TestData.WorkflowControlSetExisting.PreferenceInputPeriod;

			user.WorkflowControlSet = TestData.WorkflowControlSetExisting;
		}
	}
}