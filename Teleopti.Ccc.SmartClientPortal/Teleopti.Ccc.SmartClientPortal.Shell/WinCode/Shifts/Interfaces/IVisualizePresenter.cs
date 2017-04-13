using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
{
    public interface IVisualizePresenter : ICommon<ReadOnlyCollection<VisualPayloadInfo>>, IPresenterBase
    {
        int GetNumberOfRowsToBeShown();

        IList<int> RuleSetAmounts();

        IList<TimeSpan> ContractTimes();

        void CopyWorkShiftToSessionDataClip(int rowIndex);

		void LoadModelCollection(IWorkShiftAddCallback callback);
    }
}
