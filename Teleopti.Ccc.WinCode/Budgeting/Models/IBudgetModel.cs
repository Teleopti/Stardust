using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Models
{
    public interface IBudgetModel
    {
        IEntity ContainedEntity { get; set; }
    }
}