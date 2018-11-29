using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	[InfrastructureTest]
	public class ScheduleDifferenceSaverTest : IIsolateSystem
	{
		public IScheduleDifferenceSaver Target;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IScenarioRepository ScenarioRepository;
		public IActivityRepository ActivityRepository;
		public IPersonRepository PersonRepository;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public IScheduleStorage ScheduleStorage;
		public IEventPublisherScope Publisher;
		public IMultiplicatorDefinitionSetRepository DefinitionSetRepository;

		[Test]
		public virtual void ShouldKeepTheEventInModifiedAssignmentWhenAddingAnOvertime()
		{
			var date = new DateOnly(2000, 1, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime);
			using (var setup = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ActivityRepository.Add(activity);
				PersonRepository.Add(agent);
				ScenarioRepository.Add(scenario);
				var orgAss = new PersonAssignment(agent, scenario, date);
				orgAss.AddActivity(activity, new TimePeriod(7, 10));
				PersonAssignmentRepository.Add(orgAss);
				DefinitionSetRepository.Add(multiplicatorDefinitionSet);
				setup.PersistAll();
			}

			var eventPublisher = new LegacyFakeEventPublisher();
			using (Publisher.OnThisThreadPublishTo(eventPublisher))
			{
				using (var setup = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					var scheduleDictionary =
						ScheduleStorage.FindSchedulesForPersons(scenario, new[] {agent}, new ScheduleDictionaryLoadOptions(false, false), new DateTimePeriod(1900, 1, 1, 2100, 1, 1), new[] {agent}, false);
					//make sure we clone the assignment
					var scheduleRange = scheduleDictionary[agent];
					var scheduleDay = scheduleRange.ScheduledDay(date);
					scheduleDay.CreateAndAddOvertime(activity, new DateTimePeriod(2000, 1, 1, 10, 2000, 1, 1, 11),
						multiplicatorDefinitionSet);
					scheduleDictionary.Modify(scheduleDay, new DoNothingScheduleDayChangeCallBack());
					Target.SaveChanges(
						scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()),
						(ScheduleRange) scheduleRange);
					setup.PersistAll();

					eventPublisher.PublishedEvents.Single().Should().Be.OfType<ActivityAddedEvent>();
				}
			}

			using (var setup = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var session = setup.FetchSession();
				var personAssignment = PersonAssignmentRepository.LoadAll().First();
				session.Delete(personAssignment);
				session.Delete(multiplicatorDefinitionSet);
				session.Delete(activity);
				session.Delete(personAssignment.Person);
				session.Delete(personAssignment.Scenario);
				session.Flush();
				session.Transaction.Commit();
			}
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FullPermission>().For<IAuthorization>();
		}
	}
}