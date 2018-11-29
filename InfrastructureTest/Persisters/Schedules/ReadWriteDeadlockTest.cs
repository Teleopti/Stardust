using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	[DatabaseTest]
	public class ReadWriteDeadlockTest
	{
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IScenarioRepository ScenarioRepository;
		public IActivityRepository ActivityRepository;
		public IPersonRepository PersonRepository;
		public IFindSchedulesForPersons FindSchedulesForPersons;
		public IScheduleDictionaryPersister ScheduleDictionaryPersister;
		
		[TestCase(1)]
		[TestCase(2)]
		[TestCase(10)]
		[Ignore("Was part of #48170 but fix wasn't fixing real issue so fix rolled back. Keeping test for a while to see if it's needed. If not, remove this.")]
		public void ShouldHandleConcurrentReadsAndUpdates(int numberOfDays)
		{
			var date = new DateOnly(2000, 1, 1);
			var period = new DateOnlyPeriod(date, date.AddDays(numberOfDays));
			var activity = new Activity("_");
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			var scenario = new Scenario("_");
			using (var setup = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{					
				ScenarioRepository.Add(scenario);
				ActivityRepository.Add(activity);
				PersonRepository.Add(agent);
				foreach (var assDate in period.DayCollection())
				{
					PersonAssignmentRepository.Add(new PersonAssignment(agent, scenario, assDate).WithLayer(activity, new TimePeriod(8, 17)));
				}

				setup.PersistAll();
			}

			var tasks = new List<Task>();
			var ts = new CancellationTokenSource();
			var ct = ts.Token;
			tasks.Add(Task.Factory.StartNew(() =>
			{
				updateAgentSchedule(scenario, agent, period, activity, ct);
			}, ct));
			tasks.Add(Task.Factory.StartNew(() =>
			{
				readAgentSchedule(agent, period, scenario, ct);
			}, ct));
			Thread.Sleep(1000);
			ts.Cancel();
			Task.WaitAll(tasks.ToArray());
		}

		private void readAgentSchedule(IPerson agent1, DateOnlyPeriod period, IScenario scenario, CancellationToken ct)
		{
			while (true)
			{
				if (ct.IsCancellationRequested)
					return;
				using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					PersonAssignmentRepository.Find(new[] {agent1}, period, scenario);
				}				
			}
		}

		private void updateAgentSchedule(IScenario scenario, Person agent1, DateOnlyPeriod period, IActivity activity, CancellationToken ct)
		{
			IScheduleDictionary dic;
			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				dic = FindSchedulesForPersons.FindSchedulesForPersons(scenario, new[] {agent1},
					new ScheduleDictionaryLoadOptions(false, false), period.ToDateTimePeriod(TimeZoneInfo.Utc), new[] {agent1},
					false);
			}

			while (true)
			{
				if (ct.IsCancellationRequested)
					return;

				foreach (var scheduleDate in period.DayCollection())
				{
					var day = dic[agent1].ScheduledDay(scheduleDate);
					day.PersonAssignment().ClearMainActivities();
					day.PersonAssignment().AddActivity(activity, new TimePeriod(1, 2));
					dic.Modify(day, NewBusinessRuleCollection.Minimum());					
				}
				ScheduleDictionaryPersister.Persist(dic);
			}
		}
	}
}