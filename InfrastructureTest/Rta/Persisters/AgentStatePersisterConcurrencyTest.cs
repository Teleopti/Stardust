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
			var runner = new ConcurrencyRunner();
			var person = Guid.NewGuid();
			runner.InSync(() => Service.Prepare(person));

			runner.InParallel(() => Service.AddOne(person)).Times(20);
			runner.Wait();

			Service.Get(person).StateCode.Should().Be("20");
		}

		[Test]
		public void ShouldNotUpdateInParalellWithAll()
		{
			var runner = new ConcurrencyRunner();
			var persons = Enumerable.Range(0, 20).Select(i => Guid.NewGuid()).ToArray();
			persons.ForEach(p => Service.Prepare(p));

			runner.InParallel(() => Service.AddOneToAll());
			persons.ForEach(p => Service.AddOne(p));
			runner.Wait();

			Service.GetAll().Select(m => m.StateCode).Should().Have.SameValuesAs("2");
		}

		[Test]
		public void ShouldNotUpdateInParalellWithNotInSnapshot()
		{
			var runner = new ConcurrencyRunner();
			var persons = Enumerable.Range(0, 20).Select(i => Guid.NewGuid()).ToArray();
			persons.ForEach(p =>
			{
				Service.Prepare(p);
				Service.AddOne(p, "2016-03-15 08:00".Utc());
			});

			runner.InParallel(() => Service.AddOneToNotInSnapshot("2016-03-15 08:05".Utc()));
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
			public virtual void Prepare(Guid personId)
			{
				_persister.Prepare(new AgentStatePrepare
				{
					PersonId = personId
				}, DeadLockVictim.Yes);
			}
			
			[UnitOfWork]
			public virtual void AddOneToNotInSnapshot(DateTime snapshotId)
			{
				var personIds = _persister.FindForClosingSnapshot(snapshotId, 0, Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot);
				if (personIds.IsEmpty())
					return;
				var states = _persister.LockNLoad(personIds, DeadLockVictim.No).AgentStates;
				Thread.Sleep(TimeSpan.FromMilliseconds(100 * states.Count()));
				addOneTo(states);
			}

			[UnitOfWork]
			public virtual void AddOneToAll()
			{
				var all = _persister.ReadForTest();
				Thread.Sleep(TimeSpan.FromMilliseconds(100 * all.Count()));
				addOneTo(all);
			}

			private void addOneTo(IEnumerable<AgentState> all)
			{
				all.ForEach(m =>
				{
					m.StateCode = (int.Parse(m.StateCode ?? "0") + 1).ToString();
					_persister.Update(m);
				});
			}

			public virtual void AddOne(Guid personId)
			{
				AddOne(personId, null);
			}

			[UnitOfWork]
			public virtual void AddOne(Guid personId, DateTime? snapshotId)
			{
				var model = _persister.ReadForTest(personId).Single();
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
				model.StateCode = (int.Parse(model.StateCode ?? "0") + 1).ToString();
				model.ReceivedTime = model.ReceivedTime ?? "2016-03-15 00:00:00".Utc();
				model.SnapshotId = model.SnapshotId ?? snapshotId;
				model.SnapshotDataSourceId = 0;
				_persister.Update(model);
			}

			[UnitOfWork]
			public virtual AgentState Get(Guid personId)
			{
				return _persister.ReadForTest(personId).Single();
			}

			[UnitOfWork]
			public virtual IEnumerable<AgentState> GetAll()
			{
				return _persister.ReadForTest();
			}
		}
	}
}