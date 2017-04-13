using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
    public interface IRequestAllowanceView
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IList<BudgetAbsenceAllowanceDetailModel> DataSource { set; }

        void ReloadAbsenceSection();
    }
}