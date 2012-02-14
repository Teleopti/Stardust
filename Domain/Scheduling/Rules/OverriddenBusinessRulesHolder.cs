using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public interface IOverriddenBusinessRulesHolder
    {
        void AddOverriddenRule(IBusinessRuleResponse businessRuleResponse);
        ICollection<IBusinessRuleResponse> OverriddenRules { get; }
    }

    public class OverriddenBusinessRulesHolder : IOverriddenBusinessRulesHolder
    {
        private readonly HashSet<IBusinessRuleResponse> _overriddenRules = new HashSet<IBusinessRuleResponse>();

        public void AddOverriddenRule(IBusinessRuleResponse businessRuleResponse)
        {
            if(businessRuleResponse != null)
            {
                businessRuleResponse.Overridden = true;
                _overriddenRules.Add(businessRuleResponse);
            }
        }

        public ICollection<IBusinessRuleResponse> OverriddenRules
        {
            get { return new List<IBusinessRuleResponse>(_overriddenRules); }
        }
    }
}