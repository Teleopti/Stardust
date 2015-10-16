using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class PersonScheduleDayViewModelMapperTest
	{
		private IUserTimeZone _userTimeZone;
		private TimeZoneInfo _stockholmTimeZoneInfo;

		[SetUp]
		public void Setup()
		{
			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_stockholmTimeZoneInfo = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			_userTimeZone.Stub(x => x.TimeZone()).Return(_stockholmTimeZoneInfo);
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new PersonScheduleDayViewModelMappingProfile(_userTimeZone)));
		}
		
		[Test]
		public void ShouldMapPersonId()
		{
			var target = new PersonScheduleDayViewModelMapper();
			var personId = Guid.Empty;

			var result = target.Map(new PersonScheduleDayReadModel() { PersonId = personId });

			result.Person.Should().Be(personId);
		}

		[Test]
		public void ShouldMapDate()
		{
			var target = new PersonScheduleDayViewModelMapper();
			var date = new DateTime(2015,1,30);

			var result = target.Map(new PersonScheduleDayReadModel() { Date = date });

			result.Date.Should().Be(date);
		}

		[Test]
		public void ShouldMapStartTime()
		{
			var target = new PersonScheduleDayViewModelMapper();
			var startTime = new DateTime(2015, 1, 30, 9, 0, 0);

			var result = target.Map(new PersonScheduleDayReadModel() { Start = startTime });

			result.StartTime.Should().Be(startTime.AddHours(1));
		}		
		
		[Test]
		public void ShouldMapEndTime()
		{
			var target = new PersonScheduleDayViewModelMapper();
			var endTime = new DateTime(2015, 1, 30, 17, 0, 0);

			var result = target.Map(new PersonScheduleDayReadModel() { End = endTime });

			result.EndTime.Should().Be(endTime.AddHours(1));
		}		
		
		[Test]
		public void ShouldMapStartTimeForNull()
		{
			var target = new PersonScheduleDayViewModelMapper();

			var result = target.Map(new PersonScheduleDayReadModel() { Start = null });

			result.StartTime.Should().Be(null);
		}		
		
		[Test]
		public void ShouldMapEndTimeForNull()
		{
			var target = new PersonScheduleDayViewModelMapper();

			var result = target.Map(new PersonScheduleDayReadModel() { End = null });

			result.EndTime.Should().Be(null);
		}

		[Test]
		public void ShouldMapDayoffTrue()
		{
			var target = new PersonScheduleDayViewModelMapper();

			var result = target.Map(new PersonScheduleDayReadModel() { IsDayOff = true });

			result.IsDayOff.Should().Be(true);
		}		
		
		[Test]
		public void ShouldMapDayoffFalse()
		{
			var target = new PersonScheduleDayViewModelMapper();

			var result = target.Map(new PersonScheduleDayReadModel() { IsDayOff = false });

			result.IsDayOff.Should().Be(false);
		}
	}
}