using System;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.PayrollExportPages
{
    public class PayrollExportWizardPages : AbstractWizardPages<IPayrollExport>
    {
        public PayrollExportWizardPages(IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory) : base(repositoryFactory, unitOfWorkFactory)
        {
        }

        public override IPayrollExport CreateNewRoot()
        {
            IPayrollExport payrollExport = new PayrollExport();
            payrollExport.Name = UserTexts.Resources.PayrollExport + " " + DateTime.Now.ToString(CultureInfo.CurrentCulture);
            payrollExport.FileFormat = ExportFormat.CommaSeparated;
            return payrollExport;
        }

        public override string Name
        {
            get { return UserTexts.Resources.PayrollExportWizard; }
        }

        public override string WindowText
        {
            get { return UserTexts.Resources.CreatePayrollExport; }
        }

        public override IRepository<IPayrollExport> RepositoryObject
        {
            get { return RepositoryFactory.CreatePayrollExportRepository(UnitOfWork ); }
        }

        public override Size MinimumSize
        {
            get
            {
                return new Size(650, 550);
            }
        }
    }
}
