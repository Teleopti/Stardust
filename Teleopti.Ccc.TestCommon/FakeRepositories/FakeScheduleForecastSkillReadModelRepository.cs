using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
    public class FakeScheduleForecastSkillReadModelRepository : IScheduleForecastSkillReadModelRepository
    {
        public void Persist(IEnumerable<ResourcesDataModel> items, DateOnly date)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SkillStaffingInterval> GetBySkill(Guid skillId, DateTime startDateTime, DateTime endDateTime)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SkillStaffingInterval> GetBySkillArea(Guid skillAreaId, DateTime startDateTime, DateTime endDateTime)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastCalculatedTime()
        {
            return LastCalculatedDate;
        }

        public DateTime LastCalculatedDate { get; set; }
    }
}