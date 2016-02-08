using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
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
        private ICurrentScenario _scenario;
        private IScheduleStorage _storageFactory;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _scenario = _mocker.StrictMock<ICurrentScenario>();
            _storageFactory = _mocker.Stub<IScheduleStorage>();
            _target = new TraceableRefreshService(_scenario, _storageFactory);
        }
        
        [Test]
        public void VerifyRefreshIfNeeded()
        {
            IUnitOfWork uow = _mocker.StrictMock<IUnitOfWork>();
            IAccount pacc = _mocker.StrictMock<IAccount>();
            using(_mocker.Record())
            {
                Expect.Call(() => pacc.CalculateUsed(null, null)).IgnoreArguments().Repeat.Once();
                Expect.Call(_scenario.Current()).Repeat.Once();
            }
            using(_mocker.Playback())
            {
                _target.RefreshIfNeeded(pacc); //should only run once
                _target.RefreshIfNeeded(pacc);
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
                Expect.Call(_scenario.Current()).Repeat.Twice();
                //Expect.Call(()=>pacc.CalculateBalanceIn()).Repeat.Twice();
            }
            using (_mocker.Playback())
            {
                _target.Refresh(pacc); 
                _target.Refresh(pacc);
            }
        }
    }
}
