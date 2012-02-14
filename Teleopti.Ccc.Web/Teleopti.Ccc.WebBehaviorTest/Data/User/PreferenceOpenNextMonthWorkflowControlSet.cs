using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class StudentAvailabilityOpenNextMonthWorkflowControlSet : IUserSetup
	{
		public DateOnlyPeriod StudentAvailabilityPeriod;

		public void Apply(IPerson user, CultureInfo cultureInfo)
		{
			StudentAvailabilityPeriod = TestData.WorkflowControlSetStudentAvailabilityOpenNextMonth.StudentAvailabilityPeriod;
			user.WorkflowControlSet = TestData.WorkflowControlSetStudentAvailabilityOpenNextMonth;
		}

	}
}