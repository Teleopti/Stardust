using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class DateTimePeriodFormMappingTest
	{
		private IUserTimeZone _userTimeZone;
		
		[SetUp]
		public void Setup()
		{
			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(
				new DateTimePeriodFormMappingProfile(
					() => _userTimeZone
					)));
		}	
		
		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }
		
		[Test]
		public void ShouldMapToUtcPeriod()
		{
			var timeZone = (TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(11), "", ""));
			_userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var periodForm = new DateTimePeriodForm
			             	{
			             		StartDate = DateOnly.Today,
			             		StartTime = new TimeOfDay(TimeSpan.FromHours(12)),
			             		EndDate = DateOnly.Today,
			             		EndTime = new TimeOfDay(TimeSpan.FromHours(13))
			             	};

			var result = Mapper.Map<DateTimePeriodForm, DateTimePeriod>(periodForm);

			var expected = new DateTimePeriod(
				TimeZoneHelper.ConvertToUtc(periodForm.StartDate.Date.Add(periodForm.StartTime.Time), timeZone),
				TimeZoneHelper.ConvertToUtc(periodForm.EndDate.Date.Add(periodForm.EndTime.Time), timeZone)
				);
			result.Should().Be(expected);
		}
	}
}
