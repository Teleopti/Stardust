using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.ViewModelFactory
{
	 [TestFixture]
	 public class HeaderViewModelFactoryTest
	 {
		  private IHeaderViewModelFactory target;
		  private MockRepository mocks;
		  private IScheduleDay scheduleDay;
		  private TimeZoneInfo timeZone;
		  private DateOnly dateOnly;

		  [SetUp]
		  public void Setup()
		  {
				mocks = new MockRepository();

				scheduleDay = mocks.StrictMock<IScheduleDay>();
				timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();

				dateOnly = new DateOnly(2011,5,18);

				target = new HeaderViewModelFactory();
		  }

		  [Test, SetCulture("sv-SE"), SetUICulture("sv-SE")]
		  public void ShouldGetRegularHeaderForScheduleDay()
		  {
				using (mocks.Record())
				{
					 Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(dateOnly, timeZone));
				}
				using (mocks.Playback())
				{
					 var model = target.CreateModel(scheduleDay);

					 model.Date.Should().Be.EqualTo("2011-05-18");
					 model.Title.Should().Be.EqualTo("onsdag");
					 model.DayDescription.Should().Be.Empty();
					 model.DayNumber.Should().Be.EqualTo("18");
				}
		  }

		  [Test, SetCulture("sv-SE"), SetUICulture("sv-SE")]
		  public void ShouldGetExtendedHeaderForFirstScheduleDayOfWeek()
		  {
				using (mocks.Record())
				{
					var firstDayOfWeek = DateHelper.GetFirstDateInWeek(dateOnly, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
				Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(firstDayOfWeek, timeZone));
				}
				using (mocks.Playback())
				{
					 var model = target.CreateModel(scheduleDay);

					 model.DayDescription.Should().Be.EqualTo("maj");
				}
		  }

		  [Test, SetCulture("sv-SE"), SetUICulture("sv-SE")]
		  public void ShouldGetExtendedHeaderForFirstScheduleDayOfMonth()
		  {
				dateOnly = new DateOnly(2011,5,1);
				using (mocks.Record())
				{
					 Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(dateOnly, timeZone));
				}
				using (mocks.Playback())
				{
					 var model = target.CreateModel(scheduleDay);

					 model.DayDescription.Should().Be.EqualTo("maj");
				}
		  }
	 }
}