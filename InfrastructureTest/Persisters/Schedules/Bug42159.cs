using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	[InfrastructureTest]
	public class Bug42159 : IIsolateSystem
	{
		public IScheduleDictionaryPersister Target;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IScenarioRepository ScenarioRepository;
		public IActivityRepository ActivityRepository;
		public IPersonRepository PersonRepository;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public IScheduleStorage ScheduleStorage;

		[Test]
		public virtual void ShouldNotDoAnyUpdateIfNoRealChangesWereMade()
		{
			var date = new DateOnly(2000, 1, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			using (var setup = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ActivityRepository.Add(activity);
				PersonRepository.Add(agent);
				ScenarioRepository.Add(scenario);
				var orgAss = new PersonAssignment(agent, scenario, date);
				orgAss.AddActivity(activity, new TimePeriod(7,17));
				PersonAssignmentRepository.Add(orgAss);
				setup.PersistAll();
			}

			IScheduleDictionary scheduleDictionary;
			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				scheduleDictionary = ScheduleStorage.FindSchedulesForPersons(scenario, new [] {agent}, new ScheduleDictionaryLoadOptions(false, false), new DateTimePeriod(1900,1,1,2100,1,1), new [] {agent}, false);
			}
			//make sure we clone the assignment
			var scheduleDay = scheduleDictionary[agent].ScheduledDay(date);
			scheduleDictionary.Modify(scheduleDay, new DoNothingScheduleDayChangeCallBack());

			var assignmentBefore = scheduleDay.PersonAssignment().Version;
			Target.Persist(scheduleDictionary);

			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var assignmentAfterInDb = PersonAssignmentRepository.Get(scheduleDay.PersonAssignment().Id.Value).Version;
				assignmentBefore.Should().Be.EqualTo(assignmentAfterInDb);
			}
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FullPermission>().For<IAuthorization>();
		}
	}
}