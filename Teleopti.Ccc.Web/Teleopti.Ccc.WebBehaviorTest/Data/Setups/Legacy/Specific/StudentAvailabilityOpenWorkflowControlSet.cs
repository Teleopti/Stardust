using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class StudentAvailabilityOpenWorkflowControlSet : IUserSetup
	{
		public DateOnlyPeriod Period;
		public DateOnlyPeriod InputPeriod;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			Period = TestData.WorkflowControlSetStudentAvailabilityOpen.StudentAvailabilityPeriod;
			InputPeriod = TestData.WorkflowControlSetStudentAvailabilityOpen.StudentAvailabilityInputPeriod;
			user.WorkflowControlSet = TestData.WorkflowControlSetStudentAvailabilityOpen;
		}
	}
}