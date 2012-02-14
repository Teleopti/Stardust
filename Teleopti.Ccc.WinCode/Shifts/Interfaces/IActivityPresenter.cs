using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;

namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
{
    public interface IActivityPresenter : ICommon<IActivityViewModel>, 
                                          IPresenterBase
    {
        void AddAbsolutePositionActivity();

        void ChangeExtenderType(IActivityViewModel<ActivityNormalExtender> model, 
                                ActivityNormalExtender anExtender);

        void DeleteActivities(ReadOnlyCollection<int> selected);

        void ReOrderActivities(ReadOnlyCollection<int> adapterIndexList, MoveType moveType);
    }
}
