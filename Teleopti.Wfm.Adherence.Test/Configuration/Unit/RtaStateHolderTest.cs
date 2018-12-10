using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Unit
{
    [TestFixture]
    public class RtaStateHolderTest
    {
        private RtaStateHolder _target;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IRtaStateGroupRepository _rtaStateGroupRepository;

        [SetUp]
        public void Setup()
        {
            _schedulingResultStateHolder = new SchedulingResultStateHolder();
            _rtaStateGroupRepository = new FakeRtaStateGroupRepository(new FakeStorage());
            _target = new RtaStateHolder(_schedulingResultStateHolder, _rtaStateGroupRepository);
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
            _target = new RtaStateHolder(_schedulingResultStateHolder, _rtaStateGroupRepository);
            _target.Initialize();
			Assert.That(_target.RtaStateGroups,Is.Not.Null);
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
			var state = new AgentStateReadModel {PersonId = id};
			var state2 = new AgentStateReadModel {PersonId = Guid.NewGuid()};
			
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
            var state = new AgentStateReadModel { PersonId = id, ReceivedTime = time.AddSeconds(15)};
            var state2 = new AgentStateReadModel { PersonId = id, ReceivedTime = time};

            _target.SetActualAgentState(state);
            Assert.That(_target.ActualAgentStates.Count, Is.EqualTo(1));
            _target.SetActualAgentState(state2);
            Assert.That(_target.ActualAgentStates.Values.First(), Is.EqualTo(state));
        }

		[Test]
		public void MustHaveDefaultStateGroup()
		{
			_target = new RtaStateHolder(_schedulingResultStateHolder, _rtaStateGroupRepository);
			_target.Initialize();
			Assert.Throws<DefaultStateGroupException>(() => _target.VerifyDefaultStateGroupExists());
		}

		[Test]
		public void HaveDefaultStateGroup()
		{
			var stateGroup = new RtaStateGroup(" ");
			_rtaStateGroupRepository.Add(stateGroup);
			_target = new RtaStateHolder(_schedulingResultStateHolder, _rtaStateGroupRepository);
			_target.Initialize();
			_target.VerifyDefaultStateGroupExists();
		}
    }
}
