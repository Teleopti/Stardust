using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
{
    public interface IActivityTimeLimiterPresenter : ICommon<IActivityTimeLimiterViewModel>, 
                                                     IPresenterBase
    {
        void AddAndSaveLimiter();

        void DeleteLimiter(ReadOnlyCollection<int> selectedLimiters);

        void SetActivityTimeLimiterAdapters(IList<IActivityTimeLimiterViewModel> limiters);
    }
}
