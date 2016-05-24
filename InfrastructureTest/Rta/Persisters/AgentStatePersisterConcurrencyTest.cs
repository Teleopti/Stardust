using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[DatabaseTest]
	public class AgentStatePersisterConcurrencyTest : ISetup
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

		[Test]
		public void ShouldNotUpdateInParalellWithAll()
		{
			var runner = new ConcurrencyRunner();
			var persons = Enumerable.Range(0, 20).Select(i => Guid.NewGuid()).ToArray();
			persons.ForEach(p => Service.AddOne(p));

			runner.InParallel(() => Service.AddOneToAll());
			persons.ForEach(p => Service.AddOne(p));
			runner.Wait();

			Service.GetAll().Select(m => m.StateCode).Should().Have.SameValuesAs("3");
		}

		[Test]
		public void ShouldNotUpdateInParalellWithNotInSnapshot()
		{
			var runner = new ConcurrencyRunner();
			var persons = Enumerable.Range(0, 20).Select(i => Guid.NewGuid()).ToArray();
			persons.ForEach(p => Service.AddOne(p, "2016-03-15 08:00".Utc(), "9"));

			runner.InParallel(() => Service.AddOneToNotInSnapshot("2016-03-15 08:05".Utc(), "9"));
			persons.ForEach(p => Service.AddOne(p));
			runner.Wait();

			Service.GetAll().Select(m => m.StateCode).Should().Have.SameValuesAs("3");
		}

		public class TheService
		{
			private readonly IAgentStatePersister _persister;

			public TheService(IAgentStatePersister persister)
			{
				_persister = persister;
			}

			[UnitOfWork]
			public virtual void AddOneToNotInSnapshot(DateTime batchId, string datasourceId)
			{
				var all = _persister.GetNotInSnapshot(batchId, datasourceId);
				Thread.Sleep(TimeSpan.FromMilliseconds(100 * all.Count()));
				addOneTo(all);
			}

			[UnitOfWork]
			public virtual void AddOneToAll()
			{
				var all = _persister.GetAll();
				Thread.Sleep(TimeSpan.FromMilliseconds(100 * all.Count()));
				addOneTo(all);
			}

			private void addOneTo(IEnumerable<AgentState> all)
			{
				all.ForEach(m =>
				{
					m.StateCode = (int.Parse(m.StateCode) + 1).ToString();
					_persister.Persist(m);
				});
			}

			public virtual void AddOne(Guid personId)
			{
				AddOne(personId, null, null);
			}

			[UnitOfWork]
			public virtual void AddOne(Guid personId, DateTime? batchId, string datasourceId)
			{
				var model = _persister.Get(personId) ?? new AgentState()
				{
					PersonId = personId,
					StateCode = "0",
					ReceivedTime = "2016-03-15 00:00:00".Utc(),
					BatchId = batchId,
					SourceId = datasourceId
				};
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
				model.StateCode = (int.Parse(model.StateCode) + 1).ToString();
				_persister.Persist(model);
			}

			[UnitOfWork]
			public virtual AgentState Get(Guid personId)
			{
				return _persister.Get(personId);
			}

			[UnitOfWork]
			public virtual IEnumerable<AgentState> GetAll()
			{
				return _persister.GetAll();
			}
		}
	}
}