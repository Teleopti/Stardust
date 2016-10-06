using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
	[DatabaseTest]
	public class Bug40950DeadlockTest : DatabaseTestWithoutTransaction
	{
		private const int numberOfAgents = 10000;
		public IPersonRepository PersonRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public IScenarioRepository ScenarioRepository;

		[Test, Ignore("#40950")]
		public void ShouldNotCauseDeadlock()
		{
			IScenario scenario;
			IList<IPerson> agents;
			setupAgentsAndSchedules(out scenario, out agents);

			var cancelAssignmentReading = readAssignmentsOverAndOverAgainOnOtherThread(scenario, agents);

			try
			{
				Assert.DoesNotThrow(() =>
				{
					updateAgentsOnThisThread(agents);
				});
			}
			finally
			{
				cancelAssignmentReading.Cancel();
			}
		}

		private CancellationTokenSource readAssignmentsOverAndOverAgainOnOtherThread(IScenario scenario, IList<IPerson> agents)
		{
			var cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = cancellationTokenSource.Token;

			Task.Factory.StartNew(() =>
			{
				while (true)
				{
					using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
					{
						PersonAssignmentRepository.Find(agents, DateOnly.Today.ToDateOnlyPeriod(), scenario);
					}
					cancellationToken.ThrowIfCancellationRequested();
				}
			}, cancellationToken);
			return cancellationTokenSource;
		}

		private void updateAgentsOnThisThread(IEnumerable<IPerson> agents)
		{
			foreach (var agent in agents)
			{
				using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					agent.Name = new Name("fo0", "bar");
					PersonRepository.Add(agent);
					uow.PersistAll();
				}
			}
		}

		private void setupAgentsAndSchedules(out IScenario scenario, out IList<IPerson> agents)
		{
			var aDate = DateOnly.Today;
			scenario = new Scenario("_");
			agents = new List<IPerson>();
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				for (var i = 0; i < numberOfAgents; i++)
				{
					var agent = new Person();
					agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
					PersonRepository.Add(agent);
					agents.Add(agent);
					var ass = new PersonAssignment(agent, scenario, aDate);
					PersonAssignmentRepository.Add(ass);
				}
				uow.PersistAll();
			}
		}
	}
}