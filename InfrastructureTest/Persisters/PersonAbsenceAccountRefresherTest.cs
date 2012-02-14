using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Persisters
{
    [TestFixture]
    public class PersonAbsenceAccountRefresherTest
    {
        private MockRepository _mocks;
        private PersonAbsenceAccountRefresher _target;
        private ITraceableRefreshService _tracableRefreshService;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _tracableRefreshService = _mocks.StrictMock<ITraceableRefreshService>();
            _target = new PersonAbsenceAccountRefresher(() => _tracableRefreshService);
        }

        [Test]
        public void ShouldRefresh()
        {
            var personAbsenceAccount = _mocks.StrictMock<IPersonAbsenceAccount>();
            var account1 = _mocks.DynamicMock<IAccount>();
            var account2 = _mocks.DynamicMock<IAccount>();
            var accounts = new[] {account1, account2};
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();

            _mocks.Record();

            Expect.Call(personAbsenceAccount.AccountCollection()).Return(accounts);
            Expect.Call(() => _tracableRefreshService.Refresh(account1, unitOfWork));
            Expect.Call(() => _tracableRefreshService.Refresh(account2, unitOfWork));
            //Expect.Call(unitOfWork.DatabaseVersion(personAbsenceAccount)).Return(10);
            //Expect.Call(() => personAbsenceAccount.SetVersion(10));

            _mocks.ReplayAll();

            _target.Refresh(unitOfWork, personAbsenceAccount);

            _mocks.VerifyAll();
        }
    }
}