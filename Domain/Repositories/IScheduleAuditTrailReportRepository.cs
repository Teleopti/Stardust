using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IScheduleAuditTrailReportRepository
    {
        IList<IPerson> FindUsersFromScheduleAuditTrail();
        IList<IScheduleAuditTrailReportData> GetReportData(Guid userId, DateOnlyPeriod changePeriod, Guid scenarioId, DateOnlyPeriod schedulePeriod, string agents);
    }
}