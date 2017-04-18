using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class HandleBusinessRules
    {
        private readonly IHandleBusinessRuleResponse _handleBusinessRuleResponse;
        private readonly IViewBase _viewBase;
        private readonly IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;

        public HandleBusinessRules(IHandleBusinessRuleResponse handleBusinessRuleResponse, IViewBase viewBase, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder)
        {
            _handleBusinessRuleResponse = handleBusinessRuleResponse;
            _viewBase = viewBase;
            _overriddenBusinessRulesHolder = overriddenBusinessRulesHolder;
        }

        public IEnumerable<IBusinessRuleResponse> Handle(IEnumerable<IBusinessRuleResponse> listBusinessRuleResponse, IList<IBusinessRuleResponse> listBusinessRuleResponseToOverride)
        {
            var ret = new List<IBusinessRuleResponse>(listBusinessRuleResponseToOverride);
            var internalList = new List<IBusinessRuleResponse>();
            foreach (var businessRuleResponse in listBusinessRuleResponse)
            {
                if(!businessRuleResponse.Overridden)
                    internalList.Add(businessRuleResponse);
            }
            if (!internalList.IsEmpty() && listBusinessRuleResponseToOverride.Count == 0)
            {
                foreach (IBusinessRuleResponse response in internalList)
                {
                    if (response.Mandatory)
                    {
                        _viewBase.ShowErrorMessage(response.Message, Resources.ViolationOfABusinessRule);
                        return ret;
                    }
                }
                //show dialog to override rules
                _handleBusinessRuleResponse.SetResponse(internalList);
                if (_handleBusinessRuleResponse.DialogResult == DialogResult.Cancel)
                {
                    return ret;
                }
                // we want to override them
                foreach (IBusinessRuleResponse response in internalList)
                {
                    response.Overridden = true;
                    if(_handleBusinessRuleResponse.ApplyToAll)
                        _overriddenBusinessRulesHolder.AddOverriddenRule(response);
                }

                return internalList.Concat(ret);
            }
            return ret;
        }

		
    }
}