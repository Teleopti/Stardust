using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface IPersistConflictView
    {
        void SetupGridControl(IEnumerable<PersistConflictData> conflictCollection);
        void CloseForm();
    }
}
