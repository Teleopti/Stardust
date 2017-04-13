using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class BusinessRuleResponseViewModel
    {
        public BusinessRuleResponseViewModel(IBusinessRuleResponse businessRuleResponse)
        {
            this.Message = businessRuleResponse.Message;
            this.Name = businessRuleResponse.Person.Name.ToString();
        }

        public string Name { get; set; }
        public string Message { get; set; }
    }
}