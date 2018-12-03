using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class OpenAbsenceRequestPeriodExtractor : IOpenAbsenceRequestPeriodExtractor
    {
        private IWorkflowControlSet _workflowControlSet;
        private readonly IAbsence _absence;
        public DateOnly ViewpointDate { get; set; }

        public IEnumerable<IAbsenceRequestOpenPeriod> AvailablePeriods
        {
            get
            {
                foreach (IAbsenceRequestOpenPeriod openAbsenceRequestPeriod in _workflowControlSet.AbsenceRequestOpenPeriods)
                {
                    if (openAbsenceRequestPeriod.OpenForRequestsPeriod.Contains(ViewpointDate) && _absence.Equals(openAbsenceRequestPeriod.Absence))
                    {
                        yield return openAbsenceRequestPeriod;
                    }
                }
            }
        }

		public IEnumerable<IAbsenceRequestOpenPeriod> AllPeriods
		{
			get
			{
				foreach (IAbsenceRequestOpenPeriod openAbsenceRequestPeriod in _workflowControlSet.AbsenceRequestOpenPeriods)
				{
					if (_absence.Equals(openAbsenceRequestPeriod.Absence))
					{
						yield return openAbsenceRequestPeriod;
					}
				}
			}
		}

        public IOpenAbsenceRequestPeriodProjection Projection => new OpenAbsenceRequestPeriodProjection(this);

		public OpenAbsenceRequestPeriodExtractor(IWorkflowControlSet workflowControlSet, IAbsence absence)
        {
            ViewpointDate = DateOnly.Today;
            _workflowControlSet = workflowControlSet;
            _absence = absence;
        }
    }
}