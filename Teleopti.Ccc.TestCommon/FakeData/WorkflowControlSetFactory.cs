using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class WorkflowControlSetFactory
	{
		public static WorkflowControlSet CreateWorkFlowControlSet(IAbsence absence,
			IProcessAbsenceRequest processAbsenceRequest, bool waitlistingIsEnabled)
		{
			var startDate = new DateTime(2016, 1, 1, 00, 00, 00, DateTimeKind.Utc);
			var endDate = new DateTime(DateTime.Now.Year + 1, 12, 31, 00, 00, 00, DateTimeKind.Utc);

			var workflowControlSet = new WorkflowControlSet {AbsenceRequestWaitlistEnabled = waitlistingIsEnabled};

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod
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
