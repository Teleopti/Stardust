using System;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class WorkflowControlSetFactory
	{
		public static WorkflowControlSet CreateWorkFlowControlSet(IAbsence absence,
			IProcessAbsenceRequest processAbsenceRequest, bool waitlistingIsEnabled)
		{
			var year = DateTime.Now.Year;
			var startDate = new DateTime(year, 1, 1, 00, 00, 00, DateTimeKind.Utc);
			var endDate = new DateTime(year, 12, 31, 00, 00, 00, DateTimeKind.Utc);

			var workflowControlSet = new WorkflowControlSet {AbsenceRequestWaitlistEnabled = waitlistingIsEnabled};

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				AbsenceRequestProcess = processAbsenceRequest,
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			return workflowControlSet;
		}
	}
}
