using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [TestFixture]
    public class MeetingStateHolderLoaderHelperTest
    {
        private MockRepository _mocks;
        private IPeopleAndSkillLoaderDecider _peopleAndSkillLoaderDecider;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private MeetingStateHolderLoaderHelper _target;
        private IScenario _scenario;
        private DateTimePeriod _period;
        private IEnumerable<IPerson> _persons;
        private ISchedulerStateLoader _schedulerStateLoader;
        private IUnitOfWorkFactory _unitOfWorkFactory;
		private bool _finished;
		private ISchedulerStateHolder _schedulerStateHolder;
	    private ILoaderDeciderResult _loaderDeciderResult;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _peopleAndSkillLoaderDecider = _mocks.StrictMock<IPeopleAndSkillLoaderDecider>();
            _loaderDeciderResult = _mocks.StrictMock<ILoaderDeciderResult>();
	        _schedulerStateHolder = _mocks.DynamicMock<ISchedulerStateHolder>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _schedulerStateLoader = _mocks.StrictMock<ISchedulerStateLoader>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
	        Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(_schedulingResultStateHolder);
			_mocks.Replay(_schedulerStateHolder);
			_target = new MeetingStateHolderLoaderHelper(_peopleAndSkillLoaderDecider, _schedulerStateHolder, _schedulerStateLoader, _unitOfWorkFactory, new FakeTimeZoneGuard());

            _scenario = new Scenario("s");
            _period = new DateTimePeriod(2011,2,28,2011,3,7);
            _persons = new List<IPerson> {new Person()};
        }

        [TearDown]
        public void Teardown()
        {
            _target.Dispose();    
        }

		[Test]
        public void ShouldExecuteDeciderAndLoadWhenNewPeriod()
        {
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            _finished = false;
            _target.FinishedReloading += TargetFinishedReloading;
			_mocks.BackToRecord(_schedulerStateHolder);
            Expect.Call(_peopleAndSkillLoaderDecider.Execute(_scenario, _period, _persons)).Return(_loaderDeciderResult);
            Expect.Call(() => _schedulerStateLoader.EnsureSkillsLoaded(new DateOnlyPeriod())).IgnoreArguments();
            Expect.Call(_schedulingResultStateHolder.Skills).Return(new HashSet<ISkill>());
            Expect.Call(_loaderDeciderResult.FilterSkills(new ISkill[]{},null,null)).Return(0).IgnoreArguments();
            Expect.Call(_schedulingResultStateHolder.SkillDays).Return(new Dictionary<ISkill, IEnumerable<ISkillDay>>());
            Expect.Call(_schedulingResultStateHolder.LoadedAgents).Return(new List<IPerson>());
            Expect.Call(_loaderDeciderResult.FilterPeople(new List<IPerson>())).Return(0);
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(() => _schedulerStateLoader.LoadSchedulingResultAsync(null, null, new List<ISkill>())).IgnoreArguments();
            Expect.Call(unitOfWork.Dispose);
            _mocks.ReplayAll();
            _target.ReloadResultIfNeeded(_scenario, _period, _persons);
            var now = DateTime.Now;
            do
            {} while (!_finished && DateTime.Now < now.AddSeconds(1));
            _mocks.VerifyAll();
        }

	    void TargetFinishedReloading(object sender, ReloadEventArgs e)
        {
           _finished = true;
        }
    }
}