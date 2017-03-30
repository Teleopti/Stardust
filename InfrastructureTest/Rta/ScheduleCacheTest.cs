using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[InfrastructureTest]
	public class ScheduleCacheTest
	{
		public ScheduleCache Target;
		public ICurrentScheduleReadModelPersister Persister;
		public WithReadModelUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldRefreshAllUpdates()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();

			WithUnitOfWork.Do(() =>
			{
				Persister.Persist(person1, 1, new[] { new ScheduledActivity() });
				Persister.Persist(person2, 1, new[] { new ScheduledActivity() });
				Persister.Persist(person2, 1, new[] { new ScheduledActivity() });
			});

			Target.Refresh(0);

			WithUnitOfWork.Do(() => {
				Persister.Persist(person1, 2, new[] { new ScheduledActivity(), new ScheduledActivity() });
			});

			Target.Refresh(1);

			Target.Read(person1).Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldRefreshAllUpdates2()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();

			WithUnitOfWork.Do(() =>
			{
				Persister.Persist(person1, 1, new[] { new ScheduledActivity() });
			});

			Target.Refresh(0);

			WithUnitOfWork.Do(() => {
				Persister.Persist(person2, 2, new[] { new ScheduledActivity() });
			});

			Target.Refresh(1);

			Target.Read(person2).Should().Not.Be.Empty();
		}
	}
}