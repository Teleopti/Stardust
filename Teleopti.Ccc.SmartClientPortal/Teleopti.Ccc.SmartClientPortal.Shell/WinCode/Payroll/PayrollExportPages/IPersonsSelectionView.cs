using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.PayrollExportPages
{
    public interface IPersonsSelectionView : IPropertyPage
    {
        IApplicationFunction ApplicationFunction { get; set; }
    }
}
