using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.Service
{
	[TestFixture]
	[DatabaseTest]
	public class ScheduleCacheTest
	{
		public ScheduleCache Target;
		public ICurrentScheduleReadModelPersister Persister;
		public WithReadModelUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldRefreshAllUpdates()
		{
			var version = CurrentScheduleReadModelVersion.Generate();
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();

			WithUnitOfWork.Do(() =>
			{
				Persister.Persist(person1, 1, new[] { new ScheduledActivity() });
				Persister.Persist(person2, 1, new[] { new ScheduledActivity() });
				Persister.Persist(person2, 1, new[] { new ScheduledActivity() });
			});

			Target.Refresh(null);
			
			WithUnitOfWork.Do(() => {
				Persister.Persist(person1, 2, new[] { new ScheduledActivity(), new ScheduledActivity() });
			});

			Target.Refresh(version);

			Target.Read(person1).Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldRefreshAllUpdates2()
		{
			var version = CurrentScheduleReadModelVersion.Generate();
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();

			WithUnitOfWork.Do(() =>
			{
				Persister.Persist(person1, 1, new[] { new ScheduledActivity() });
			});

			Target.Refresh(null);

			WithUnitOfWork.Do(() => {
				Persister.Persist(person2, 2, new[] { new ScheduledActivity() });
			});

			Target.Refresh(version);

			Target.Read(person2).Should().Not.Be.Empty();
		}
	}
}