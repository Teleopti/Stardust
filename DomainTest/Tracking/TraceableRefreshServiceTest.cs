using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Tracking
{

    [TestFixture]
    public class TraceableRefreshServiceTest
    {
        private MockRepository _mocker;
        private ITraceableRefreshService _target;
        private IScenario _scenario;
        private IRepositoryFactory _repositoryFactory;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _scenario = _mocker.StrictMock<IScenario>();
            _repositoryFactory = _mocker.Stub<IRepositoryFactory>();
            _target = new TraceableRefreshService(_scenario, _repositoryFactory);
        }
        
        [Test]
        public void VerifyRefreshIfNeeded()
        {
            IUnitOfWork uow = _mocker.StrictMock<IUnitOfWork>();
            IAccount pacc = _mocker.StrictMock<IAccount>();
            using(_mocker.Record())
            {
                Expect.Call(() => pacc.CalculateUsed(null, null)).IgnoreArguments().Repeat.Once();
            }
            using(_mocker.Playback())
            {
                _target.RefreshIfNeeded(pacc, uow); //should only run once
                _target.RefreshIfNeeded(pacc, uow);
            }
        }

        [Test]
        public void VerifyRefresh()
        {
            IUnitOfWork uow = _mocker.StrictMock<IUnitOfWork>();
            IAccount pacc = _mocker.StrictMock<IAccount>();
            using (_mocker.Record())
            {
                Expect.Call(() => pacc.CalculateUsed(null, null)).IgnoreArguments().Repeat.Twice();
                //Expect.Call(()=>pacc.CalculateBalanceIn()).Repeat.Twice();
            }
            using (_mocker.Playback())
            {
                _target.Refresh(pacc, uow); 
                _target.Refresh(pacc, uow);
            }
        }
    }
}
