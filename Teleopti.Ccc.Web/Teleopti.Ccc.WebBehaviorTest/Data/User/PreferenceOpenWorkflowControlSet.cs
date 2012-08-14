using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class PreferenceOpenWorkflowControlSet : IUserSetup
	{
		public DateOnlyPeriod Period;
		public DateOnlyPeriod InputPeriod;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			Period = TestData.WorkflowControlSetPreferenceOpen.PreferencePeriod;
			InputPeriod = TestData.WorkflowControlSetPreferenceOpen.PreferenceInputPeriod;
			user.WorkflowControlSet = TestData.WorkflowControlSetPreferenceOpen;
		}
	}
}