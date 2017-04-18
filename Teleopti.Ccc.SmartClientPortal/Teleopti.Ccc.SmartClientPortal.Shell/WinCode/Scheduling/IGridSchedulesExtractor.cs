using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public interface IGridSchedulesExtractor
    {
        IList<IScheduleDay> Extract();
        IList<IScheduleDay> ExtractSelected();
    }
}
