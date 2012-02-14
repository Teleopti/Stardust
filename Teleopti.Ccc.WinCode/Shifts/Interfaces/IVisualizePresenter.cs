using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
{
    public interface IVisualizePresenter : ICommon<ReadOnlyCollection<VisualPayloadInfo>>, IPresenterBase
    {
        int GetNumberOfRowsToBeShown();

        IList<int> RuleSetAmounts();

        IList<TimeSpan> ContractTimes();

        void CopyWorkShiftToSessionDataClip(int rowIndex);
    }
}
