using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
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
		private const int numberOfAgents = 200;
		private const int numberOfOpeningScheduler = 100;
		public IPersonRepository PersonRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public IScenarioRepository ScenarioRepository;

		[Test, Ignore("Failing for #40950")]
		public void ShouldNotCauseDeadlock()
		{
			IScenario scenario;
			IList<IPerson> agents;
			setupAgentsAndSchedules(out scenario, out agents);

			var cancelAssignmentReading = updatePersonsOverAndOverAgainOnOtherThread(agents);

			try
			{
				Assert.DoesNotThrow(() =>
				{
					for (var i = 0; i < numberOfOpeningScheduler; i++)
					{
						using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
						{
							//TODO: let's see if we could use "real code" here
							PersonRepository.LoadAll();
							PersonAssignmentRepository.Find(new DateOnlyPeriod(DateOnly.Today.AddDays(-1), DateOnly.Today.AddDays(1)), scenario);
						}
					}
				});
			}
			finally
			{
				cancelAssignmentReading.Cancel();
			}
		}

		private CancellationTokenSource updatePersonsOverAndOverAgainOnOtherThread(IEnumerable<IPerson> agents)
		{
			var cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = cancellationTokenSource.Token;

			Task.Factory.StartNew(() =>
			{
				var rnd = new Random();
				while (true)
				{
					foreach (var agentsToUpdate in agents.Reverse().Batch(1))
					{
						using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
						{
							foreach (var agent in agentsToUpdate)
							{
								agent.Name = new Name(rnd.Next().ToString(), rnd.Next().ToString());
								PersonRepository.Add(agent);
							}
							uow.PersistAll();
						}
					}
					cancellationToken.ThrowIfCancellationRequested();
					}
			}, cancellationToken);
			return cancellationTokenSource;
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