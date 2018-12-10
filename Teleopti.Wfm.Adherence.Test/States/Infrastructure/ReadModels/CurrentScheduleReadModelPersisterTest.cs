using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.ReadModels
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
			Target.Persist(Guid.NewGuid(), 1, new[] {new ScheduledActivity()});

			Reader.Read(0).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldFetchWithProperties()
		{
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
			Target.Persist(person, 1, schedule);

			var result = Reader.Read(0).Single();
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
				Target.Persist(personId, 1, Enumerable.Range(0, 100).Select(i => new ScheduledActivity()));
			});
			Assert.DoesNotThrow(() =>
			{
				Target.Persist(personId, 2, Enumerable.Range(0, 100).Select(i => new ScheduledActivity()));
			});
		}

		[Test]
		public void ShouldReplaceOnPersist()
		{
			var person = Guid.NewGuid();
			Target.Persist(person, 1, new[] { new ScheduledActivity(), new ScheduledActivity() });
			Target.Persist(person, 2, new[] { new ScheduledActivity() });

			Reader.Read(0).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPersistEmptySchedule()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Target.Persist(person1, 1, new[] { new ScheduledActivity() });
			Target.Persist(person2, 2, new[] { new ScheduledActivity() });
			Target.Persist(person2, 3, Enumerable.Empty<ScheduledActivity>());

			var result = Reader.Read(0);
			result.Single(x => x.PersonId == person1).Schedule.Should().Have.Count.EqualTo(1);
			result.Single(x => x.PersonId == person2).Schedule.Should().Be.Empty();
		}

		[Test]
		public void ShouldPersistSchedule()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Target.Persist(person, 1, new[]
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

			var result = Reader.Read(0).Single().Schedule.Single();
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

			Reader.Read(0).Should().Be.Empty();
		}

		[Test]
		public void ShouldInvalidateOneOfTwo()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Target.Persist(person1, 1, new[] { new ScheduledActivity() });
			Target.Persist(person2, 2, new[] { new ScheduledActivity() });

			Target.Invalidate(person1);

			Target.GetInvalid().Single().Should().Be(person1);
		}

		[Test]
		public void ShouldBeValidOnPersist()
		{
			var person = Guid.NewGuid();

			Target.Persist(person, 1, new[] { new ScheduledActivity() });
			Target.Invalidate(person);
			Target.Persist(person, 2, new[] { new ScheduledActivity(), new ScheduledActivity() });

			Target.GetInvalid().Should().Be.Empty();
		}
		
		[Test]
		public void ShouldOnlyReadUpdates()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();

			Target.Persist(person1, 1, new[] { new ScheduledActivity() });
			Target.Persist(person2, 1, new[] { new ScheduledActivity() });
			Target.Persist(person2, 2, new[] { new ScheduledActivity() });

			Reader.Read(1).Single().PersonId.Should().Be(person2);
		}

		[Test]
		public void ShouldReadAllWhithUnspecifiedScheduleVersion()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();

			Target.Persist(person1, 1, new[] { new ScheduledActivity() });
			Target.Persist(person2, 1, new[] { new ScheduledActivity() });
			Target.Persist(person2, 2, new[] { new ScheduledActivity() });

			Reader.Read().Select(x => x.PersonId).Should().Have.SameValuesAs(person1, person2);
		}
		
	}
}