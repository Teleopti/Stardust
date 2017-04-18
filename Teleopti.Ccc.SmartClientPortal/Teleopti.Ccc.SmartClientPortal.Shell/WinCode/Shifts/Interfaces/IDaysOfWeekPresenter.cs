using System.Collections.Generic;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces
{
    public interface IDaysOfWeekPresenter : ICommon<IDaysOfWeekViewModel>, 
                                            IPresenterBase
    {
        void SetDaysOfWeekCollection(IList<IDaysOfWeekViewModel> value);
    }
}
