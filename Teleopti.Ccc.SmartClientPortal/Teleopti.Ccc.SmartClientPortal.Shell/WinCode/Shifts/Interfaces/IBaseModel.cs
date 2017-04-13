using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
{
    public interface IBaseModel : IValidate
    {
        IWorkShiftRuleSet WorkShiftRuleSet { get; }

        Description WorkShiftRuleSetName { get; set; }        
    }
    
    public interface IBaseModel<T> : IBaseModel
    {
        T ContainedEntity { get;}
    }
}
