using System;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[DatabaseTest]
	public class AgentStateReadModelPersisterConcurrencyTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TheService>();
		}

		public TheService Service { get; set; }

		[Test]
		public void ShouldNotUpdateInParalell()
		{
			var personId = Guid.NewGuid();
			var runner = new ConcurrencyRunner();
			runner.InSync(() => Service.AddOne(personId));

			runner.InParallel(() => Service.AddOne(personId)).Times(20);
			runner.Wait();

			Service.Get(personId).StateCode.Should().Be("21");
		}
		
		public class TheService
		{
			private readonly IAgentStateReadModelPersister _persister;

			public TheService(IAgentStateReadModelPersister persister)
			{
				_persister = persister;
			}

			[AnalyticsUnitOfWork]
			public virtual void AddOne(Guid personId)
			{
				var model = _persister.Get(personId) ?? new AgentStateReadModel
				{
					PersonId = personId,
					StateCode = "0",
					ReceivedTime = "2016-03-15 00:00:00".Utc()
				};
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
				model.StateCode = (int.Parse(model.StateCode) + 1).ToString();
				_persister.Persist(model);
			}

			[AnalyticsUnitOfWork]
			public virtual AgentStateReadModel Get(Guid personId)
			{
				return _persister.Get(personId);
			}
		}
	}
}