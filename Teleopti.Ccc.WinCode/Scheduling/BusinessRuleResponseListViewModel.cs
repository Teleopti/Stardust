using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class BusinessRuleResponseListViewModel
    {
        public BusinessRuleResponseListViewModel(IEnumerable<IBusinessRuleResponse> businessRuleResponses)
        {
            BusinessRuleResponses = (from r in businessRuleResponses select new BusinessRuleResponseViewModel(r)).ToArray();
        }

        public IEnumerable<BusinessRuleResponseViewModel> BusinessRuleResponses { get; set; }
    }
}