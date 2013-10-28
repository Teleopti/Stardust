using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
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