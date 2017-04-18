using System.Collections.ObjectModel;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces
{
    public interface IAccessibilityDatePresenter : ICommon<IAccessibilityDateViewModel>, 
                                                   IPresenterBase
    {
        void AddAccessibilityDate();

        void RemoveSelectedAccessibilityDates(ReadOnlyCollection<int> dates);

        void SetAccessibilityDates(ReadOnlyCollection<IAccessibilityDateViewModel> dates);
    }
}
