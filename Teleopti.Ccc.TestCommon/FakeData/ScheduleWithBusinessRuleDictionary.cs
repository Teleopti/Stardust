using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public class ScheduleWithBusinessRuleDictionary : ScheduleDictionary
    {
        private readonly List<IBusinessRuleResponse> _businessRuleReponseCollection = new List<IBusinessRuleResponse>();

        protected ScheduleWithBusinessRuleDictionary(IScenario scenario, IScheduleDateTimePeriod period, IDictionary<IPerson, IScheduleRange> dictionary) : base(scenario, period, dictionary)
        {
        }

        public ScheduleWithBusinessRuleDictionary(IScenario scenario, IScheduleDateTimePeriod period, IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService) : base(scenario, period, differenceCollectionService)
        {
        }

        public ScheduleWithBusinessRuleDictionary(IScenario scenario, IScheduleDateTimePeriod period) : base(scenario, period)
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
