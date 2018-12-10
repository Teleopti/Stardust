using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces
{
    public interface IActivityTimeLimiterPresenter : ICommon<IActivityTimeLimiterViewModel>, 
                                                     IPresenterBase
    {
        void AddAndSaveLimiter();

        void DeleteLimiter(ReadOnlyCollection<int> selectedLimiters);
    }
}
