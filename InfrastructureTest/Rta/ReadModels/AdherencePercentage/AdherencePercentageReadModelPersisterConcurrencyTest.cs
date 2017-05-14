using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AdherencePercentage
{
	[TestFixture]
	[DatabaseTest]
	public class AdherencePercentageReadModelPersisterConcurrencyTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TheService>();
		}

		public TheService Service { get; set; }

		[Test]
		public void ShouldNotRetryWhenMultipleWorkersUpdatesSameModel()
		{
			var date = "2016-02-08".Date();
			var personId = Guid.NewGuid();
			var simulator = new RetryingQueueSimulator();

			simulator.ProcessAsync(() => Service.AddState(date, personId));
			simulator.WaitForAll();

			20.Times(() => simulator.ProcessAsync(() => Service.AddState(date, personId)));
			simulator.WaitForAll();

			simulator.RetryCount.Should().Be(0);
			Service.Get(date, personId).State.Should().Have.Count.EqualTo(21);
		}

		[Test]
		public void ShouldCalculateCorrectlyWhenMultipleWorkersAddTheSameModel()
		{
			var date = "2016-02-08".Date();
			var personId = Guid.NewGuid();
			var simulator = new RetryingQueueSimulator();

			20.Times(() => simulator.ProcessAsync(() => Service.AddState(date, personId)));
			simulator.WaitForAll();

			Service.Get(date, personId).State.Should().Have.Count.EqualTo(20);
		}

		public class TheService
		{
			private readonly IAdherencePercentageReadModelPersister _persister;

			public TheService(IAdherencePercentageReadModelPersister persister)
			{
				_persister = persister;
			}

			[ReadModelUnitOfWork]
			public virtual void AddState(DateOnly date,Guid personId)
			{
				var model = _persister.Get(date, personId);
				if (model == null)
					model = new AdherencePercentageReadModel
					{
						Date = date.Date,
						PersonId = personId
					};
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
				model.State = model.State.EmptyIfNull().Concat(new[] {new AdherencePercentageReadModelState()}).ToArray();
				_persister.Persist(model);
			}

			[ReadModelUnitOfWork]
			public virtual AdherencePercentageReadModel Get(DateOnly date, Guid personId)
			{
				return _persister.Get(date, personId);
			}
		}
		
	}
}