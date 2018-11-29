using System;
using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;


namespace Teleopti.Ccc.WebTest.Core.WeekSchedule
{
	[TestFixture]
	public class ScheduleHtmlHelperTest
	{
		private HtmlHelper helper;

		[SetUp]
		public void Setup()
		{
			helper = new TestHtmlHelperBuilder().CreateHtmlHelper(new ViewDataDictionary());
		}
		
		[Test]
		public void ShouldReturnStyleClassTodayIfToday() 
		{
			var result = helper.Schedule().StyleClassToday(new DayViewModelBase {Date = DateOnly.Today});
			result.Should().Be("today");
		}

		[Test]
		public void ShouldReturnStyleClassTodayNullIfNotToday()
		{
			var result = helper.Schedule().StyleClassToday(new DayViewModelBase { Date = DateOnly.Today.AddDays(1) });
			result.Should().Be.Empty();
		}

		[Test, SetCulture("en-us"), SetUICulture("en-us")]
		public void FormatDateShouldRenderCorrectFormatInEnUsCulture()
		{
			var dateOnly = DateOnly.Today;

			var result = helper.Schedule().FormatDate(dateOnly);
			result.Should().Be.EqualTo(dateOnly.Date.ToString("yyyy-MM-dd"));
		}

		[Test]
		public void FormatDateShouldRenderCorrectFormatInAnyCulture()
		{
			var dateOnly = DateOnly.Today;

			var result = helper.Schedule().FormatDate(dateOnly);
			result.Should().Be.EqualTo(dateOnly.Date.ToString("yyyy-MM-dd"));
		}

		[Test]
		public void FormatStateShouldRenderIntForState()
		{
			const SpecialDateState state = SpecialDateState.Today;

			var result = Enum.Parse(typeof(SpecialDateState), helper.Schedule().FormatState(state));
			result.Should().Be.EqualTo(state);
		}
		[Test, SetCulture("en-us"), SetUICulture("en-us")]
		public void ShouldCorrectlySerializePeriodSelectionToJson()
		{
			var expectedResult =  "{\"Date\": \"2011-05-21\",\"StartDate\":\"1/1/2011\",\"EndDate\":\"12/31/2011\",\"Display\": \"2011-05-16 - 2011-05-22\",\"SelectedDateRange\": {\"MinDate\": \"2011-05-16\",\"MaxDate\": \"2011-05-22\"},\"SelectableDateRange\": {\"MinDate\": \"2011-01-01\",\"MaxDate\": \"2011-12-31\"},\"PeriodNavigation\": {\"NextPeriod\": \"2011-05-23\", \"HasNextPeriod\": true,\"PrevPeriod\": \"2011-05-14\",\"HasPrevPeriod\": false,\"CanPickPeriod\": true}}".Replace(" ", string.Empty);
			var model = new PeriodSelectionViewModel
			                                 	{
			                                 		Date = new DateOnly(2011, 05, 21).ToFixedClientDateOnlyFormat(),
													StartDate = new DateTime(2011, 01, 01),
													EndDate = new DateTime(2011, 12, 31),
			                                 		Display = "2011-05-16 - 2011-05-22",
			                                 		SelectedDateRange =
			                                 			new PeriodDateRangeViewModel
														{
															MinDate = new DateOnly(2011, 05, 16).ToFixedClientDateOnlyFormat()
																,
															MaxDate = new DateOnly(2011, 05, 22).ToFixedClientDateOnlyFormat()
														},
			                                 		SelectableDateRange =
			                                 			new PeriodDateRangeViewModel { MinDate = new DateOnly(2011, 01, 01).ToFixedClientDateOnlyFormat(),
																					   MaxDate = new DateOnly(2011, 12, 31).ToFixedClientDateOnlyFormat()
														},
			                                 		PeriodNavigation =
			                                 			new PeriodNavigationViewModel
			                                 				{
			                                 					NextPeriod = new DateOnly(2011, 05, 23).ToFixedClientDateOnlyFormat(),
			                                 					HasNextPeriod = true,
			                                 					PrevPeriod = new DateOnly(2011, 05, 14).ToFixedClientDateOnlyFormat(),
			                                 					HasPrevPeriod = false,
			                                 					CanPickPeriod = true
			                                 				}
			                                 	};

			var result = helper.Schedule().PeriodSelectionAsJson(model);

			result.Replace(" ", string.Empty).Should().Be.EqualTo(expectedResult);
		}
	}
}