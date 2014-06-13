using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class CreateHourTextTest
	{
		[Test, SetCulture("sv-SE")]
		public void ShouldCreateHourTextInSwedish()
		{
			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();

			userTimeZone.Stub(x => x.TimeZone()).Return(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

			var target = new CreateHourText(new CurrentThreadUserCulture(), userTimeZone);

			var result = target.CreateText(new DateTime(2013, 9, 30, 8, 0, 0, DateTimeKind.Utc));

			result.Should().Be.EqualTo("10:00");
		}

		[Test, SetCulture("en-US")]
		public void ShouldCreateHourTextInEnglishUnitedStates()
		{
			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();

			userTimeZone.Stub(x => x.TimeZone()).Return(TimeZoneInfo.FindSystemTimeZoneById("UTC"));

			var target = new CreateHourText(new CurrentThreadUserCulture(), userTimeZone);

			var result = target.CreateText(new DateTime(2013, 9, 30, 8, 0, 0, DateTimeKind.Utc));

			result.Should().Be.EqualTo("8 AM");
		}
	}
}