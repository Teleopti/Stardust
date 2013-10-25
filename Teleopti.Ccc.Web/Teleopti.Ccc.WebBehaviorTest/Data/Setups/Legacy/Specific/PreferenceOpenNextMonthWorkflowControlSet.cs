using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class StudentAvailabilityOpenNextMonthWorkflowControlSet : IUserSetup
	{
		public DateOnlyPeriod StudentAvailabilityPeriod;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			StudentAvailabilityPeriod = TestData.WorkflowControlSetStudentAvailabilityOpenNextMonth.StudentAvailabilityPeriod;
			user.WorkflowControlSet = TestData.WorkflowControlSetStudentAvailabilityOpenNextMonth;
		}

	}
}