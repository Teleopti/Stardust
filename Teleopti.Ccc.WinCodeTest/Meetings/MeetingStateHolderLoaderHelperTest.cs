using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Meetings;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
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

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _peopleAndSkillLoaderDecider = _mocks.StrictMock<IPeopleAndSkillLoaderDecider>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _schedulerStateLoader = _mocks.StrictMock<ISchedulerStateLoader>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _target = new MeetingStateHolderLoaderHelper(_peopleAndSkillLoaderDecider, _schedulingResultStateHolder, _schedulerStateLoader, _unitOfWorkFactory);

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
            Expect.Call(() =>_peopleAndSkillLoaderDecider.Execute(_scenario, _period, _persons));
            Expect.Call(() => _schedulerStateLoader.EnsureSkillsLoaded());
            Expect.Call(_schedulingResultStateHolder.Skills).Return(new List<ISkill>());
            Expect.Call(_peopleAndSkillLoaderDecider.FilterSkills(new HashSet<ISkill>())).Return(0).IgnoreArguments();
            Expect.Call(_schedulingResultStateHolder.SkillDays).Return(new Dictionary<ISkill, IList<ISkillDay>>());
            Expect.Call(_schedulingResultStateHolder.PersonsInOrganization).Return(new List<IPerson>());
            Expect.Call(_peopleAndSkillLoaderDecider.FilterPeople(new List<IPerson>())).Return(0);
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(() => _schedulerStateLoader.LoadSchedulingResultAsync(null, unitOfWork, null,new List<ISkill>())).IgnoreArguments();
            Expect.Call(unitOfWork.Dispose);
            _mocks.ReplayAll();
            _target.ReloadResultIfNeeded(_scenario, _period, _persons);
            var now = DateTime.Now;
            do
            {} while (!_finished && DateTime.Now < now.AddSeconds(1));
            _mocks.VerifyAll();
        }

        private bool _finished;
        void TargetFinishedReloading(object sender, ReloadEventArgs e)
        {
           _finished = true;
        }
    }

    
}