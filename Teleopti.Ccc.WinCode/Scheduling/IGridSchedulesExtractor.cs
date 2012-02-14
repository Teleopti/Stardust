using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface IGridSchedulesExtractor
    {
        IList<IScheduleDay> Extract();
        IList<IScheduleDay> ExtractSelected();
    }
}
