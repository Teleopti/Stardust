using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
    /// <summary>
    /// Represents the IPersonAccountModel.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 8/29/2008
    /// </remarks>
    public interface IPersonAccountModel
    {
        int PersonAccountCount { get; }
        DateOnly? AccountDate { get; set; }

        string FullName { get; }
        string AccountType { get; }
        IAbsence TrackingAbsence { get; set; }

        bool CanGray { get; }
        bool ExpandState { get; set; }

        object BalanceIn { get; set; }
        object Used { get; }
        object BalanceOut { get; set; }
        object Extra { get; set; }
        object Accrued { get; set; }
        object Remaining { get; }

        GridControl GridControl { get; set; }
        IPersonAccountCollection Parent { get; }
        IAccount CurrentAccount { get; }
        bool CanBold { get; set; }
        void ResetCanBoldPropertyOfChildAdapters();
    }
}
