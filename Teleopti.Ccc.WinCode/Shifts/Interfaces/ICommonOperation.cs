using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Payroll;

namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
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
