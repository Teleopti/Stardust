using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.PayrollExportPages;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Payroll.PayrollExportPages
{
    [TestFixture]
    public class PayrollExportPropertiesPagesTest : IDisposable
    {
        private PayrollExportPropertiesPages _target;
        private IPayrollExport _payrollExport;
        private MockRepository _mocks;
        private IRepositoryFactory _repositoryFactory;
        private IUnitOfWorkFactory _unitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _payrollExport = new PayrollExport();
            _payrollExport.Name = "Test";
            _mocks = new MockRepository();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _target = new PayrollExportPropertiesPages(_payrollExport,_repositoryFactory,_unitOfWorkFactory);
        }

        [Test]
        public void VerifyCreateRootIsNotPossible()
        {
            Assert.Throws<NotImplementedException>(() => _target.CreateNewRoot());
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsEmpty(_target.Name);
            Assert.IsNotEmpty(_target.WindowText);
        }

        [Test]
        public void VerifyRepositoryObject()
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
                IRepository<IPayrollExport> payrollExportRepository = _target.RepositoryObject;
                Assert.IsNotNull(payrollExportRepository);
            }
            _mocks.BackToRecord(uow);
            Expect.Call(uow.Dispose);
            _mocks.Replay(uow);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _target.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
