using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Performance
{	
	[DatabaseTest]
	public class NumberOfDbCallsForAssignmentPersistTest : IIsolateSystem
	{
		public IScheduleDictionaryPersister Target;
		public IScheduleStorage ScheduleStorage;
		public IPersonRepository PersonRepository;
		public IScenarioRepository ScenarioRepository;
		public IActivityRepository ActivityRepository;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;

		[Test]
		public void ShouldBatchStatementsEffectivly()
		{
			var scenario = new Scenario();
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(DateOnly.Today, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			var activity = new Activity("_");
			IScheduleDictionary schedules;
			using (var setupUow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(agent);
				ActivityRepository.Add(activity);
				foreach (var date in period.DayCollection())
				{
					ScheduleStorage.Add(new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(7, 17)));
				}
				setupUow.PersistAll();
				schedules = ScheduleStorage.FindSchedulesForPersons(scenario, new[] {agent},
					new ScheduleDictionaryLoadOptions(false, false),
					period.ToDateTimePeriod(agent.PermissionInformation.DefaultTimeZone()), new[] {agent}, false);
			}
			foreach (var date in period.DayCollection())
			{
				var scheduleDay = schedules[agent].ScheduledDay(date);
				scheduleDay.PersonAssignment().AddActivity(activity, new TimePeriod(8, 9));
				schedules.Modify(scheduleDay, NewBusinessRuleCollection.Minimum(), true);
			}

			var sessionFactory = CurrentUnitOfWorkFactory.Current().FetchSessionFactory();
			using (sessionFactory.WithStats())
			{
				Target.Persist(schedules);

				//One select of version numbers
				//one select to read activities
				//one batch of insert of all layers
				//one batch of update of all assignments
				//(currently for some strange reason) one batch of update of all layers 
				sessionFactory.Statistics.PrepareStatementCount
					.Should().Be.IncludedIn(1, 5);
			}
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FullPermission>().For<IAuthorization>();
		}
	}
}