using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public class ScheduleWithBusinessRuleDictionary : ScheduleDictionary
    {
        private readonly List<IBusinessRuleResponse> _businessRuleReponseCollection = new List<IBusinessRuleResponse>();

        public ScheduleWithBusinessRuleDictionary(IScenario scenario, IScheduleDateTimePeriod period) : base(scenario, period, new PersistableScheduleDataPermissionChecker())
        {
        }

        public void AddBusinessRule(IBusinessRuleResponse businessRuleResponse)
        {
            _businessRuleReponseCollection.Add(businessRuleResponse);
        }

        protected override IEnumerable<IBusinessRuleResponse> CheckIfCanModify(Dictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleParts, INewBusinessRuleCollection newBusinessRules)
        {
            return _businessRuleReponseCollection;
        }
    }
}
