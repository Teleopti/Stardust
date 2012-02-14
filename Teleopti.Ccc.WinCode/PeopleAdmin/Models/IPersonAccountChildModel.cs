using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
    public interface IPersonAccountChildModel
    {
        IPersonAccountCollection Parent { get; }
        IAccount ContainedEntity { get; }

        bool CanGray { get; }
        string FullName { get; }
        string AccountType { get; }
        IAbsence TrackingAbsence { get; set; }

        object BalanceIn { get; set; }
        object Used { get; }
        object BalanceOut { get; set; }
        object Remaining { get; }
        DateOnly? AccountDate { get; set; }
        object Extra { get; set; }
        object Accrued { get; set; }
        bool CanBold { get; set; }
    }
}
