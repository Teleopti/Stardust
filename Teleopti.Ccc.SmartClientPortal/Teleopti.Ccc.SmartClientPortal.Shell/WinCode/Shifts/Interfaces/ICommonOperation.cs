using System.Collections.Generic;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces
{
    public interface ICommonOperation
    {
        void Add();

        void Delete();

        void Sort(SortingMode mode);

        void Cut();

        void Copy();

        void Paste();

        void RefreshView();

        void Amounts(IList<int> shiftAmount);
    }
}
