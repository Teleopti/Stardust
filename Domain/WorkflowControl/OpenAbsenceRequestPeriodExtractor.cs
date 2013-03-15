using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

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

        public IOpenAbsenceRequestPeriodProjection Projection
        {
            get { return new OpenAbsenceRequestPeriodProjection(this); }
        }

        public IWorkflowControlSet WorkflowControlSet
        {
            get { return _workflowControlSet; }
        }

        public OpenAbsenceRequestPeriodExtractor(IWorkflowControlSet workflowControlSet, IAbsence absence)
        {
            ViewpointDate = DateOnly.Today;
            _workflowControlSet = workflowControlSet;
            _absence = absence;
        }
    }
}