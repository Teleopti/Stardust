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

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[DatabaseTest]
	public class AdherenceDetailsReadModelPersisterConcurrencyTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TheService>();
		}

		public TheService Service { get; set; }

		[Test]
		public void ShouldCalculateCorrectlyWhenMultipleWorkersUpdatesSameModel()
		{
			var date = "2016-02-08".Date();
			var personId = Guid.NewGuid();
			var simulator = new RetryingQueueSimulator();

			simulator.ProcessAsync(() => Service.AddState(date, personId));
			simulator.WaitForAll();

			20.Times(() => simulator.ProcessAsync(() => Service.AddState(date, personId)));
			simulator.WaitForAll();

			Service.Get(date, personId).State.Adherence.Should().Have.Count.EqualTo(21);
		}

		[Test]
		public void ShouldCalculateCorrectlyWhenMultipleWorkersAddTheSameModel()
		{
			var date = "2016-02-08".Date();
			var personId = Guid.NewGuid();
			var simulator = new RetryingQueueSimulator();

			20.Times(() => simulator.ProcessAsync(() => Service.AddState(date, personId)));
			simulator.WaitForAll();

			Service.Get(date, personId).State.Adherence.Should().Have.Count.EqualTo(20);
		}

		public class TheService
		{
			private readonly IAdherenceDetailsReadModelPersister _persister;

			public TheService(IAdherenceDetailsReadModelPersister persister)
			{
				_persister = persister;
			}

			[ReadModelUnitOfWork]
			public virtual void AddState(DateOnly date,Guid personId)
			{
				var model = _persister.Get(date, personId);
				if (model == null)
					model = new AdherenceDetailsReadModel
					{
						Date = date.Date,
						PersonId = personId
					};
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
				model.State = model.State ?? new AdherenceDetailsReadModelState();
				model.State.Adherence = model.State.Adherence.EmptyIfNull().Concat(new[] { new AdherenceDetailsReadModelAdherenceState() }).ToArray();
				_persister.Persist(model);
			}

			[ReadModelUnitOfWork]
			public virtual AdherenceDetailsReadModel Get(DateOnly date, Guid personId)
			{
				return _persister.Get(date, personId);
			}
		}
		
	}
}