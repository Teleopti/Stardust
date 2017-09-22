using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.PayrollExportPages;

namespace Teleopti.Ccc.WinCodeTest.Payroll.PayrollExportPages
{
    [TestFixture]
    public class PayrollExportWizardPagesTest
    {
        private PayrollExportWizardPages _target;
        private MockRepository _mocks;
        private IRepositoryFactory _repositoryFactory;
        private IUnitOfWorkFactory _unitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();

            _target = new PayrollExportWizardPages(_repositoryFactory,_unitOfWorkFactory);
        }

        [Test]
        public void CanCreateInstance()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.IsNotNull(_target.Name);
            Assert.IsNotNull(_target.WindowText);
            Size minSize = _target.MinimumSize;
            Assert.AreEqual(650, minSize.Width);
            Assert.AreEqual(550, minSize.Height);
        }

        [Test]
        public void CanCreateNewRoot()
        {
            IPayrollExport payrollExport = _target.CreateNewRoot();
            Assert.IsNotNull(payrollExport);
            Assert.AreEqual(ExportFormat.CommaSeparated, payrollExport.FileFormat);
        }

        [Test]
        public void CanGetRepositoryObject()
        {
            IUnitOfWork uow = _mocks.DynamicMock<IUnitOfWork>();
            IPayrollExportRepository repository = _mocks.DynamicMock<IPayrollExportRepository>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                Expect.Call(_repositoryFactory.CreatePayrollExportRepository(uow)).Return(repository);
            }
            using (_mocks.Playback())
            {
                IRepository<IPayrollExport> obj = _target.RepositoryObject;
                Assert.IsNotNull(obj);
            }
        }
    }
}
