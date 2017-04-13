using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
{
    public interface IDaysOfWeekPresenter : ICommon<IDaysOfWeekViewModel>, 
                                            IPresenterBase
    {
        void SetDaysOfWeekCollection(IList<IDaysOfWeekViewModel> value);
    }
}
