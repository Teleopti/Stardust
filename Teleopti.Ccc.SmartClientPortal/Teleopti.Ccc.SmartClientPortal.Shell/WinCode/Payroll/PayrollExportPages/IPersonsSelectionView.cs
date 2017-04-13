using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Payroll.PayrollExportPages
{
    public interface IPersonsSelectionView : IPropertyPage
    {
        IApplicationFunction ApplicationFunction { get; set; }
    }
}
