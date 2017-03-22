using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[ReadModelUnitOfWorkTest]
	public class CurrentScheduleReadModelPersisterTest
	{
		public Database Database;
		public MutableNow Now;
		public ICurrentScheduleReadModelPersister Target;
		public IScheduleReader Reader;

		[Test]
		public void ShouldPersistOne()
		{
			Now.Is("2017-03-23 08:00");
			Target.Persist(Guid.NewGuid(), new[] {new ScheduledActivity()});

			Reader.Read(Now.UtcDateTime()).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldFetchWithProperties()
		{
			Now.Is("2017-03-23 08:00");
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var schedule = new[]
			{
				new ScheduledActivity
				{
					PersonId = person,
					PayloadId = phone,
					BelongsToDate = "2017-03-23".Date(),
					StartDateTime = "2017-03-23 08:00".Utc(),
					EndDateTime = "2017-03-23 11:00".Utc(),
					Name = "phone",
					ShortName = "ph",
					DisplayColor = Color.DarkGoldenrod.ToArgb()
				}
			};
			Target.Persist(person, schedule);

			var result = Reader.Read(Now.UtcDateTime()).Single();
			result.PersonId.Should().Be(person);
			result.Schedule.Single().PersonId.Should().Be(person);
			result.Schedule.Single().PayloadId.Should().Be(phone);
			result.Schedule.Single().BelongsToDate.Should().Be("2017-03-23".Date());
			result.Schedule.Single().StartDateTime.Should().Be("2017-03-23 08:00".Utc());
			result.Schedule.Single().EndDateTime.Should().Be("2017-03-23 11:00".Utc());
			result.Schedule.Single().Name.Should().Be("phone");
			result.Schedule.Single().ShortName.Should().Be("ph");
			result.Schedule.Single().DisplayColor.Should().Be(Color.DarkGoldenrod.ToArgb());
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
			Now.Is("2017-03-23 08:00");
			var person = Guid.NewGuid();
			Target.Persist(person, new[] { new ScheduledActivity(), new ScheduledActivity() });
			Target.Persist(person, new[] { new ScheduledActivity() });

			Reader.Read(Now.UtcDateTime()).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPersistEmptySchedule()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Target.Persist(person1, new[] { new ScheduledActivity() });
			Target.Persist(person2, new[] { new ScheduledActivity() });
			Target.Persist(person2, Enumerable.Empty<ScheduledActivity>());

			var result = Reader.Read(Now.UtcDateTime());
			result.Single(x => x.PersonId == person1).Schedule.Should().Have.Count.EqualTo(1);
			result.Single(x => x.PersonId == person2).Schedule.Should().Be.Empty();
		}

		[Test]
		public void ShouldPersistSchedule()
		{
			Now.Is("2017-03-23 08:00");
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

			var result = Reader.Read(Now.UtcDateTime()).Single().Schedule.Single();
			result.PersonId.Should().Be(person);
			result.BelongsToDate.Should().Be("2017-01-27".Date());
			result.PayloadId.Should().Be(phone);
			result.Name.Should().Be("Phone");
			result.ShortName.Should().Be("P");
			result.DisplayColor.Should().Be(Color.Green.ToArgb());
			result.StartDateTime.Should().Be("2017-01-27 08:00".Utc());
			result.EndDateTime.Should().Be("2017-01-27 17:00".Utc());
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

			Reader.Read(Now.UtcDateTime()).Should().Be.Empty();
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