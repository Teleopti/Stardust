using NUnit.Framework;
using SharpTestsEx;
using System;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Notification
{
	[TestFixture]
	[DomainTest]
	public class TeamScheduleWeekViewChangeCheckerTest : IIsolateSystem
	{
		public ITeamScheduleWeekViewChangeChecker Target;
		public FakeScheduleDayReadModelRepository FakeScheduleDayReadModelRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<TeamScheduleWeekViewChangeChecker>().For<ITeamScheduleWeekViewChangeChecker>();
		}

		[Test]
		public void ShouldBeRelevantChangeIfTheScheduleIsChangedToWorkingDayFromDayOff()
		{
			var date = new DateOnly(2019, 01, 16);
			var person = PersonFactory.CreatePerson("person", "1");
			var newReadModel = new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 16, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			};

			FakeScheduleDayReadModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 16, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = false,
				Date = date.Date
			});

			var result = Target.IsRelevantChange(date, person, newReadModel);

			result.Should().Be.True();
		}

		[Test]
		public void ShouldBeRelevantChangeIfTheScheduleIsChangedToDayOffFromWorkingDay()
		{
			var date = new DateOnly(2019, 01, 16);
			var person = PersonFactory.CreatePerson("person", "1");
			var newReadModel = new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 0, 0, 0),
				EndDateTime = new DateTime(2019, 01, 17, 0, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = false,
				Date = date.Date
			};

			FakeScheduleDayReadModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 16, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			});

			var result = Target.IsRelevantChange(date, person, newReadModel);

			result.Should().Be.True();
		}

		[Test]
		public void ShouldBeRelevantChangeIfTheScheduleStartTimeIsChanged() {
			var date = new DateOnly(2019, 01, 16);
			var person = PersonFactory.CreatePerson("person", "1");
			var newReadModel = new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 7, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 17, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			};

			FakeScheduleDayReadModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 17, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			});

			var result = Target.IsRelevantChange(date, person, newReadModel);

			result.Should().Be.True();
		}

		[Test]
		public void ShouldBeRelevantChangeIfTheScheduleEndTimeIsChanged()
		{
			var date = new DateOnly(2019, 01, 16);
			var person = PersonFactory.CreatePerson("person", "1");
			var newReadModel = new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 17, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			};

			FakeScheduleDayReadModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 16, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			});

			var result = Target.IsRelevantChange(date, person, newReadModel);

			result.Should().Be.True();
		}

		[Test]
		public void ShouldBeRelevantChangeIfTheScheduleDescriptionIsChanged()
		{
			var date = new DateOnly(2019, 01, 16);
			var person = PersonFactory.CreatePerson("person", "1");
			var newReadModel = new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 17, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date,
				Label = "Early"
			};

			FakeScheduleDayReadModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 17, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date,
				Label = "Late"
			});

			var result = Target.IsRelevantChange(date, person, newReadModel);

			result.Should().Be.True();
		}

		[Test]
		public void ShouldNotBeRelevantChange() {
			var date = new DateOnly(2019, 01, 16);
			var person = PersonFactory.CreatePerson("person", "1");
			var newReadModel = new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 17, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			};

			FakeScheduleDayReadModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2019, 01, 16, 8, 0, 0),
				EndDateTime = new DateTime(2019, 01, 16, 17, 0, 0),
				PersonId = person.Id.GetValueOrDefault(),
				Workday = true,
				Date = date.Date
			});

			var result = Target.IsRelevantChange(date, person, newReadModel);

			result.Should().Be.False();
		}


	}
}
