using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.PayrollExportPages
{
    public class PayrollExportPropertiesPages : AbstractPropertyPages<IPayrollExport>
    {
        public PayrollExportPropertiesPages(IPayrollExport payrollExport, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
            : base(payrollExport,repositoryFactory,unitOfWorkFactory)
        {
        }

        public override IPayrollExport CreateNewRoot()
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { return String.Empty; }
        }

        public override string WindowText
        {
            get { return UserTexts.Resources.Properties; }
        }

        public override IRepository<IPayrollExport> RepositoryObject
        {
            get { return RepositoryFactory.CreatePayrollExportRepository(UnitOfWork); }
        }
    }
}
