using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class ExistingWorkflowControlSet : IUserSetup
	{
		public DateOnlyPeriod StudentAvailabilityPeriod;
		public DateOnlyPeriod StudentAvailabilityInputPeriod;
		public DateOnlyPeriod PreferencePeriod;
		public DateOnlyPeriod PreferenceInputPeriod;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			StudentAvailabilityPeriod = TestData.WorkflowControlSetExisting.StudentAvailabilityPeriod;
			StudentAvailabilityInputPeriod = TestData.WorkflowControlSetExisting.StudentAvailabilityInputPeriod;
			PreferencePeriod = TestData.WorkflowControlSetExisting.PreferencePeriod;
			PreferenceInputPeriod = TestData.WorkflowControlSetExisting.PreferenceInputPeriod;

			user.WorkflowControlSet = TestData.WorkflowControlSetExisting;
		}
	}
}