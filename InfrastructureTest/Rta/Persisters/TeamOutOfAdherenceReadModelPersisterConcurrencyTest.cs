using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[DatabaseTest]
	public class TeamOutOfAdherenceReadModelPersisterConcurrencyTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TheService>();
		}

		public TheService Service { get; set; }

		[Test]
		public void ShouldHandleMultipleWorkersUpdatingTheSameModel()
		{
			var teamId = Guid.NewGuid();
			var simulator = new RetryingQueueSimulator();

			simulator.ProcessAsync(() => Service.GetAndIncrementCount(teamId));
			simulator.WaitForAll();

			10.Times(() => simulator.ProcessAsync(() => Service.GetAndIncrementCount(teamId)));
			10.Times(() => simulator.ProcessAsync(() => Service.GetAllAndIncrementCount(teamId)));
			simulator.WaitForAll();

			Service.Get(teamId).Count.Should().Be(21);
		}

		[Test]
		public void ShouldHandle2WorkersAddingTheSameModel()
		{
			var teamId = Guid.NewGuid();
			var simulator = new RetryingQueueSimulator();

			simulator.ProcessAsync(() => Service.GetAndIncrementCount(teamId));
			simulator.ProcessAsync(() => Service.GetAllAndIncrementCount(teamId));
			simulator.WaitForAll();

			Service.Get(teamId).Count.Should().Be(2);
		}

		public class TheService
		{
			private readonly ITeamOutOfAdherenceReadModelPersister _persister;

			public TheService(ITeamOutOfAdherenceReadModelPersister persister)
			{
				_persister = persister;
			}

			[ReadModelUnitOfWork]
			public virtual void GetAndIncrementCount(Guid teamId)
			{
				incrementCount(_persister.Get(teamId), teamId);
			}

			[ReadModelUnitOfWork]
			public virtual void GetAllAndIncrementCount(Guid teamId)
			{
				incrementCount(_persister.GetAll().SingleOrDefault(), teamId);
			}

			private void incrementCount(TeamOutOfAdherenceReadModel model, Guid teamId)
			{
				if (model == null)
					model = new TeamOutOfAdherenceReadModel
					{
						TeamId = teamId
					};
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
				model.Count++;
				_persister.Persist(model);
			}

			[ReadModelUnitOfWork]
			public virtual TeamOutOfAdherenceReadModel Get(Guid teamId)
			{
				return _persister.Get(teamId);
			}
		}
		
	}
}