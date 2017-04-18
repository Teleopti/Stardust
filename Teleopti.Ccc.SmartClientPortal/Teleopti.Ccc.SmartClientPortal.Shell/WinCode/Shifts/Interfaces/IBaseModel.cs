using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces
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
