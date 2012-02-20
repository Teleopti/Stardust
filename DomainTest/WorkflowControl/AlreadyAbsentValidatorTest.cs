using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
	public class AlreadyAbsentValidatorTest
    {
        private IAbsenceRequestValidator target;
        private MockRepository mocks;
    	private IScenario scenario;
    	private IPerson person;

    	[SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        	scenario = ScenarioFactory.CreateScenarioAggregate();
    		person = PersonFactory.CreatePerson();
			target = new AlreadyAbsentValidator();
        }

        [Test]
        public void ShouldExplainInvalidReason()
        {
            Assert.AreEqual("AlreadyAbsent", target.InvalidReason);
        }

        [Test]
        public void ShouldHaveDisplayText()
        {
			Assert.AreEqual("AlreadyAbsent", target.DisplayText);
        }

        [Test]
        public void ShouldBeValidIfNoPreviousAbsence()
        {
        	var schedulePart =
        		new SchedulePartFactoryForDomain(person, scenario, new DateTimePeriod(2012, 2, 20, 2012, 2, 21),
												 SkillFactory.CreateSkill("test")).CreatePartWithMainShift();
        			
        	var stateHolder = mocks.DynamicMock<ISchedulingResultStateHolder>();

			var personRequest = new PersonRequest(person);
			var absenceRequest = new AbsenceRequest(AbsenceFactory.CreateAbsence("Sick leave"), schedulePart.Period);
			personRequest.Request = absenceRequest;
			var range = mocks.DynamicMock<IScheduleRange>();

			using (mocks.Record())
			{
				Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(2012, 2, 20, 2012, 2, 20))).Return(new[] { schedulePart });
				Expect.Call(stateHolder.Schedules).Return(schedulePart.Owner);
			}
			using (mocks.Playback())
			{
				((ScheduleDictionaryForTest)schedulePart.Owner).AddTestItem(person, range);
				target.SchedulingResultStateHolder = stateHolder;

				Assert.True(target.Validate(absenceRequest));
			}
        }

		[Test]
		public void ShouldBeInvalidIfAbsenceExistsOnShift()
		{
			var schedulePart =
				new SchedulePartFactoryForDomain(person, scenario, new DateTimePeriod(2012, 2, 20, 2012, 2, 22),
				                                 SkillFactory.CreateSkill("test")).CreateSchedulePartWithMainShiftAndAbsence();

			var stateHolder = mocks.DynamicMock<ISchedulingResultStateHolder>();

			var personRequest = new PersonRequest(person);
			var absenceRequest = new AbsenceRequest(AbsenceFactory.CreateAbsence("Sick leave"), schedulePart.Period);
			personRequest.Request = absenceRequest;
			var range = mocks.DynamicMock<IScheduleRange>();

			using (mocks.Record())
			{
				Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(2012, 2, 20, 2012, 2, 20))).Return(new[] {schedulePart});
				Expect.Call(stateHolder.Schedules).Return(schedulePart.Owner);
			}
			using (mocks.Playback())
			{
				((ScheduleDictionaryForTest)schedulePart.Owner).AddTestItem(person,range);
				target.SchedulingResultStateHolder = stateHolder;

				Assert.IsFalse(target.Validate(absenceRequest));
			}
		}

    	[Test]
        public void ShouldCreateNewInstance()
        {
            var newInstance = target.CreateInstance();
            Assert.AreNotSame(target, newInstance);
            Assert.IsTrue(typeof(AlreadyAbsentValidator).IsInstanceOfType(newInstance));
        }

        [Test]
        public void ShouldAllInstancesBeEqual()
        {
            var otherValidatorOfSameKind = new AlreadyAbsentValidator();
            Assert.IsTrue(otherValidatorOfSameKind.Equals(target));
        }

        [Test]
        public void ShouldNotEqualIfTheyAreInstancesOfDifferentType()
        {
            var otherValidator = new AbsenceRequestNoneValidator();
            Assert.IsFalse(target.Equals(otherValidator));
        }
    }
}
