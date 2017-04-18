using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models
{
    public interface IBudgetModel
    {
        IEntity ContainedEntity { get; set; }
    }
}