using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.Service.AgentState
{
	[TestFixture]
	[DatabaseTest]
	public class AgentStatePersisterConcurrencyTest : IExtendSystem
	{		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<TheService>();
		}

		public TheService Service { get; set; }
		public MutableNow Now { get; set; }

		[Test]
		public void ShouldNotUpdateInParalell()
		{
			var runner = new ConcurrencyRunner();
			var person = Guid.NewGuid();
			Now.Is("2016-03-15 00:00:00");
			runner.InSync(() => Service.Prepare(person));

			runner.InParallel(() => Service.AddOneSecond(person)).Times(20);
			runner.Wait();

			Service.Get(person).ReceivedTime.Should().Be("2016-03-15 00:00:20".Utc());
		}

		[Test]
		public void ShouldNotUpdateInParalellWithAll()
		{
			var runner = new ConcurrencyRunner();
			var persons = Enumerable.Range(0, 20).Select(i => Guid.NewGuid()).ToArray();
			Now.Is("2016-03-15 00:00:00");
			persons.ForEach(p => Service.Prepare(p));

			runner.InParallel(() => Service.AddOneSecondToAll());
			persons.ForEach(p => Service.AddOneSecond(p));
			runner.Wait();

			Service.GetAll().Select(m => m.ReceivedTime).Should().Have.SameValuesAs("2016-03-15 00:00:02".Utc());
		}

		[Test]
		public void ShouldNotUpdateInParalellWithNotInSnapshot()
		{
			var runner = new ConcurrencyRunner();
			var persons = Enumerable.Range(0, 20).Select(i => Guid.NewGuid()).ToArray();
			var state = Guid.NewGuid();
			Now.Is("2016-03-15 00:00:00");
			persons.ForEach(p =>
			{
				Service.Prepare(p);
				Service.AddOneSecond(p, state, "2016-03-15 08:00".Utc());
			});

			runner.InParallel(() => Service.AddOneToNotInSnapshot("2016-03-15 08:05".Utc()));
			persons.ForEach(p => Service.AddOneSecond(p));
			runner.Wait();

			Service.GetAll().Select(m => m.ReceivedTime).Should().Have.SameValuesAs("2016-03-15 00:00:03".Utc());
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
					PersonId = personId,
				}, DeadLockVictim.Yes);
			}
			
			[UnitOfWork]
			public virtual void AddOneToNotInSnapshot(DateTime snapshotId)
			{
				var personIds = _persister.FindForClosingSnapshot(snapshotId, 0, new Guid[] {});
				if (personIds.IsEmpty())
					return;
				var states = _persister.LockNLoad(personIds, DeadLockVictim.No).AgentStates;
				Thread.Sleep(TimeSpan.FromMilliseconds(100 * states.Count()));
				addOneTo(states);
			}

			[UnitOfWork]
			public virtual void AddOneSecondToAll()
			{
				var all = _persister.ReadForTest();
				Thread.Sleep(TimeSpan.FromMilliseconds(100 * all.Count()));
				addOneTo(all);
			}

			private void addOneTo(IEnumerable<Adherence.States.AgentState> all)
			{
				all.ForEach(model =>
				{
					model.ReceivedTime = (model.ReceivedTime ?? "2016-03-15 00:00:00".Utc()).AddSeconds(1);
					_persister.Update(model);
				});
			}

			public virtual void AddOneSecond(Guid personId)
			{
				AddOneSecond(personId, null, null);
			}

			[UnitOfWork]
			public virtual void AddOneSecond(Guid personId, Guid? stateGroupId, DateTime? snapshotId)
			{
				var model = _persister.ReadForTest(personId).Single();
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
				model.ReceivedTime = (model.ReceivedTime ?? "2016-03-15 00:00:00".Utc()).AddSeconds(1);
				model.StateGroupId = model.StateGroupId ?? stateGroupId;
				model.SnapshotId = model.SnapshotId ?? snapshotId;
				model.SnapshotDataSourceId = 0;
				_persister.Update(model);
			}

			[UnitOfWork]
			public virtual Adherence.States.AgentState Get(Guid personId)
			{
				return _persister.ReadForTest(personId).Single();
			}

			[UnitOfWork]
			public virtual IEnumerable<Adherence.States.AgentState> GetAll()
			{
				return _persister.ReadForTest();
			}
		}
	}
}