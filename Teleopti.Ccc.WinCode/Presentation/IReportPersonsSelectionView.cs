using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Presentation
{
    public interface IReportPersonsSelectionView
    {
        HashSet<Guid> SelectedAgentGuids();
    }
}