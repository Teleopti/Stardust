using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence
{
    [TestFixture]
    public class RtaStateHolderTest
    {
        private MockRepository _mocks;
        private RtaStateHolder _target;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;

        private IRtaStateGroupRepository _rtaStateGroupRepository;
        private IStateGroupActivityAlarmRepository _stateGroupActivityAlarmRepository;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();

            _rtaStateGroupRepository = _mocks.StrictMock<IRtaStateGroupRepository>();
            _stateGroupActivityAlarmRepository = _mocks.StrictMock<IStateGroupActivityAlarmRepository>();

            _target = new RtaStateHolder(_schedulingResultStateHolder, _rtaStateGroupRepository,
                                         _stateGroupActivityAlarmRepository);
        }

        [Test]
        public void VerifyProperties()
        {
	        var persons = new List<IPerson>();
			_target.SetFilteredPersons(persons);
            Assert.AreEqual(0, _target.ActualAgentStates.Count);
            Assert.AreEqual(_schedulingResultStateHolder, _target.SchedulingResultStateHolder);
			Assert.That(_target.FilteredPersons,Is.EqualTo(persons));
        }

        [Test]
        public void ShouldLoadStateOnInit()
        {
	        Expect.Call(_rtaStateGroupRepository.LoadAllCompleteGraph()).Return(new List<IRtaStateGroup>());
	        Expect.Call(_stateGroupActivityAlarmRepository.LoadAllCompleteGraph())
				.Return(new List<IStateGroupActivityAlarm>());
            
            _mocks.ReplayAll();
            _target = new RtaStateHolder(_schedulingResultStateHolder, _rtaStateGroupRepository, _stateGroupActivityAlarmRepository);
            _target.Initialize();
			Assert.That(_target.RtaStateGroups,Is.Not.Null);
            _mocks.VerifyAll();
        }

		[Test]
		public void ShouldSetStateOnPerson()
		{
			var id = Guid.NewGuid();
			var person = new Person();
			person.SetId(id);
			var persons = new List<IPerson>{person};
			_target.SetFilteredPersons(persons);
			Assert.AreEqual(0, _target.ActualAgentStates.Count);
			var state = new ActualAgentState {PersonId = id};
			var state2 = new ActualAgentState {PersonId = Guid.NewGuid()};
			
			_target.SetActualAgentState(state);
			Assert.That(_target.ActualAgentStates.Count,Is.EqualTo(1));
			_target.SetActualAgentState(state);
			Assert.That(_target.ActualAgentStates.Count, Is.EqualTo(1));
			_target.SetActualAgentState(state2);
			Assert.That(_target.ActualAgentStates.Count, Is.EqualTo(1));
		}

        [Test]
        public void ShouldSetKeepNewerStateOnPerson()
        {
            var id = Guid.NewGuid();
            var person = new Person();
            person.SetId(id);
            var persons = new List<IPerson> { person };
            _target.SetFilteredPersons(persons);
            Assert.AreEqual(0, _target.ActualAgentStates.Count);

            var time = DateTime.UtcNow;
            var state = new ActualAgentState { PersonId = id, ReceivedTime = time.AddSeconds(15)};
            var state2 = new ActualAgentState { PersonId = id, ReceivedTime = time};

            _target.SetActualAgentState(state);
            Assert.That(_target.ActualAgentStates.Count, Is.EqualTo(1));
            _target.SetActualAgentState(state2);
            Assert.That(_target.ActualAgentStates.Values.First(), Is.EqualTo(state));
        }

		[Test, ExpectedException(typeof(DefaultStateGroupException))]
		public void MustHaveDefaultStateGroup()
		{
			Expect.Call(_rtaStateGroupRepository.LoadAllCompleteGraph()).Return(new List<IRtaStateGroup>());
			Expect.Call(_stateGroupActivityAlarmRepository.LoadAllCompleteGraph())
				.Return(new List<IStateGroupActivityAlarm>());

			_mocks.ReplayAll();
			_target = new RtaStateHolder(_schedulingResultStateHolder, _rtaStateGroupRepository, _stateGroupActivityAlarmRepository);
			_target.Initialize();
			_target.VerifyDefaultStateGroupExists();
			_mocks.VerifyAll();
		}

		[Test]
		public void HaveDefaultStateGroup()
		{
			var stateGroup = _mocks.DynamicMock<IRtaStateGroup>();
			Expect.Call(_rtaStateGroupRepository.LoadAllCompleteGraph()).Return(new List<IRtaStateGroup>{stateGroup});
			Expect.Call(_stateGroupActivityAlarmRepository.LoadAllCompleteGraph())
				.Return(new List<IStateGroupActivityAlarm>());

			_mocks.ReplayAll();
			_target = new RtaStateHolder(_schedulingResultStateHolder, _rtaStateGroupRepository, _stateGroupActivityAlarmRepository);
			_target.Initialize();
			_target.VerifyDefaultStateGroupExists();
			_mocks.VerifyAll();
		}
    }
}
