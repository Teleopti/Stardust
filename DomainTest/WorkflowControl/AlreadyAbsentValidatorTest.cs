using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture][Ignore("To be fixed")]
	public class AlreadyAbsentValidatorTest
    {
		private AlreadyAbsentSpecification target;
        private MockRepository mocks;
    	private IScenario scenario;
    	private IPerson person;
    	private ISchedulingResultStateHolder stateHolder;

    	[SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        	scenario = ScenarioFactory.CreateScenarioAggregate();
			person = PersonFactory.CreatePerson();
			stateHolder = mocks.DynamicMock<ISchedulingResultStateHolder>();
			target = new AlreadyAbsentSpecification(new AlreadyAbsentValidator(new FakeGlobalSettingDataRepository()));
        }

        [Test]
        public void ShouldNotBeSatisfiedIfNoPreviousAbsence()
        {
        	var schedulePart =
        		new SchedulePartFactoryForDomain(person, scenario, new DateTimePeriod(2012, 2, 20, 2012, 2, 21),
												 SkillFactory.CreateSkill("test")).CreatePartWithMainShift();
        	
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
				
				Assert.False(target.IsSatisfiedBy(new AbsenceRequstAndSchedules(absenceRequest, stateHolder, new BudgetGroupState())));
			}
        }

		[Test]
		public void ShouldBeSatisfiedIfAbsenceExistsOnShift()
		{
			var schedulePart =
				new SchedulePartFactoryForDomain(person, scenario, new DateTimePeriod(2012, 2, 20, 2012, 2, 22),
				                                 SkillFactory.CreateSkill("test")).CreateSchedulePartWithMainShiftAndAbsence();

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
				
				Assert.True(target.IsSatisfiedBy(new AbsenceRequstAndSchedules(absenceRequest, stateHolder, new BudgetGroupState())));
			}
		}
    }
}
