using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation
{
    public interface IReportPersonsSelectionView
    {
        HashSet<Guid> SelectedAgentGuids();
    }
}