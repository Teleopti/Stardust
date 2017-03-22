using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[ReadModelUnitOfWorkTest]
	public class CurrentScheduleReadModelPersisterTest
	{
		public Database Database;
		public ICurrentScheduleReadModelPersister Target;
		public IScheduleReader Reader;

		[Test]
		public void ShouldPersistOne()
		{
			Target.Persist(Guid.NewGuid(), new[] {new ScheduledActivity()});

			Reader.Read().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPersistLargeSchedule()
		{
			var personId = Guid.NewGuid();
			Assert.DoesNotThrow(() =>
			{
				Target.Persist(personId, Enumerable.Range(0, 100).Select(i => new ScheduledActivity()));
			});
			Assert.DoesNotThrow(() =>
			{
				Target.Persist(personId, Enumerable.Range(0, 100).Select(i => new ScheduledActivity()));
			});
		}

		[Test]
		public void ShouldReplaceOnPersist()
		{
			var person = Guid.NewGuid();
			Target.Persist(person, new[] { new ScheduledActivity(), new ScheduledActivity() });
			Target.Persist(person, new[] { new ScheduledActivity() });

			Reader.Read().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldDeleteOnPersistEmpty()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Target.Persist(person1, new[] { new ScheduledActivity() });
			Target.Persist(person2, new[] { new ScheduledActivity() });
			Target.Persist(person2, Enumerable.Empty<ScheduledActivity>());

			Reader.Read().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPersistSchedule()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Target.Persist(person, new[]
			{
				new ScheduledActivity
				{
					PersonId = person,
					BelongsToDate = "2017-01-27".Date(),
					PayloadId = phone,
					Name = "Phone",
					ShortName = "P",
					DisplayColor = Color.Green.ToArgb(),
					StartDateTime = "2017-01-27 08:00".Utc(),
					EndDateTime = "2017-01-27 17:00".Utc()
				}
			});

			Reader.Read().Single().PersonId.Should().Be(person);
			Reader.Read().Single().BelongsToDate.Should().Be("2017-01-27".Date());
			Reader.Read().Single().PayloadId.Should().Be(phone);
			Reader.Read().Single().Name.Should().Be("Phone");
			Reader.Read().Single().ShortName.Should().Be("P");
			Reader.Read().Single().DisplayColor.Should().Be(Color.Green.ToArgb());
			Reader.Read().Single().StartDateTime.Should().Be("2017-01-27 08:00".Utc());
			Reader.Read().Single().EndDateTime.Should().Be("2017-01-27 17:00".Utc());
		}

		[Test]
		public void ShouldInvalidate()
		{
			var person = Guid.NewGuid();

			Target.Invalidate(person);

			Target.GetInvalid().Single().Should().Be(person);
		}

		[Test]
		public void ShouldReadEmptyAfterInvalidation()
		{
			var person = Guid.NewGuid();

			Target.Invalidate(person);

			Reader.Read().Should().Be.Empty();
		}

		[Test]
		public void ShouldInvalidateOneOfTwo()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Target.Persist(person1, new[] { new ScheduledActivity() });
			Target.Persist(person2, new[] { new ScheduledActivity() });

			Target.Invalidate(person1);

			Target.GetInvalid().Single().Should().Be(person1);
		}

		[Test]
		public void ShouldBeValidOnPersist()
		{
			var person = Guid.NewGuid();

			Target.Persist(person, new[] { new ScheduledActivity() });
			Target.Invalidate(person);
			Target.Persist(person, new[] { new ScheduledActivity(), new ScheduledActivity() });

			Target.GetInvalid().Should().Be.Empty();
		}


	}
}