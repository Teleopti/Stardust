using System.Globalization;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class StudentAvailabilityClosedWorkflowControlSet : IUserSetup
	{
		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			user.WorkflowControlSet = TestData.WorkflowControlSetStudentAvailabilityClosed;
		}
	}
}