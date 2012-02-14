using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class StudentAvailabilityOpenWorkflowControlSet : IUserSetup
	{
		public DateOnlyPeriod Period;
		public DateOnlyPeriod InputPeriod;

		public void Apply(IPerson user, CultureInfo cultureInfo)
		{
			Period = TestData.WorkflowControlSetStudentAvailabilityOpen.StudentAvailabilityPeriod;
			InputPeriod = TestData.WorkflowControlSetStudentAvailabilityOpen.StudentAvailabilityInputPeriod;
			user.WorkflowControlSet = TestData.WorkflowControlSetStudentAvailabilityOpen;
		}
	}
}