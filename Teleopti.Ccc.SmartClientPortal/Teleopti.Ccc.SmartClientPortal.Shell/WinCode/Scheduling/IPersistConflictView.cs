using System.Collections.Generic;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public interface IPersistConflictView
    {
        void SetupGridControl(IEnumerable<PersistConflictData> conflictCollection);
        void CloseForm();
    }
}
