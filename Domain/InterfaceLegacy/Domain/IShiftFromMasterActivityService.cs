using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Interface for ShiftFromMasterActivityService
    /// </summary>
    public interface IShiftFromMasterActivityService
    {
        IList<IWorkShift> ExpandWorkShiftsWithMasterActivity(IWorkShift workShift, bool baseIsMaster);
    }
}
