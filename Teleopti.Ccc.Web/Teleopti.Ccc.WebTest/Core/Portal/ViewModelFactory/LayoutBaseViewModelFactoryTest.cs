using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	public class LayoutBaseViewModelFactoryTest
	{
		private ILayoutBaseViewModelFactory _target;
		private MockRepository _mocks;
		private ICultureSpecificViewModelFactory _cultureSpecificViewModelFactory;
		private IDatePickerGlobalizationViewModelFactory _datePickerGlobalizationViewModelFactory;
		private IUserTimeZone _userTimeZone;

		[SetUp]
		public void Setup()
		{

			_mocks = new MockRepository();
			_cultureSpecificViewModelFactory = _mocks.DynamicMock<ICultureSpecificViewModelFactory>();
			_datePickerGlobalizationViewModelFactory = _mocks.DynamicMock<IDatePickerGlobalizationViewModelFactory>();
			_userTimeZone = new UtcTimeZone();
			_target = new LayoutBaseViewModelFactory(_cultureSpecificViewModelFactory, _datePickerGlobalizationViewModelFactory, new Now(), _userTimeZone);
		}

		[Test]
		public void ShouldCreateModel()
		{
			var cultureSpecificViewModel = new CultureSpecificViewModel();
			var datePickerGlobalizationViewModel = new DatePickerGlobalizationViewModel();
			using (_mocks.Record())
			{
				Expect.Call(_cultureSpecificViewModelFactory.CreateCutureSpecificViewModel()).Return(
					cultureSpecificViewModel);
				Expect.Call(_datePickerGlobalizationViewModelFactory.CreateDatePickerGlobalizationViewModel()).Return(
					datePickerGlobalizationViewModel);
			}

			using (_mocks.Playback())
			{
				var result = _target.CreateLayoutBaseViewModel(string.Empty);

				result.Should().Not.Be.Null();
				result.CultureSpecific.Should().Be.SameInstanceAs(cultureSpecificViewModel);
				result.DatePickerGlobalization.Should().Be.SameInstanceAs(datePickerGlobalizationViewModel);
			}
		}

		[Test]
		public void ShouldSetTime()
		{
			var time = new DateTime(2001, 1, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var now = new MutableNow();
			now.Is(time);

			var target = new LayoutBaseViewModelFactory(
				_cultureSpecificViewModelFactory,
				_datePickerGlobalizationViewModelFactory,
				now,_userTimeZone);
			
			target.CreateLayoutBaseViewModel(string.Empty).FixedDate.Should().Be.EqualTo(time);
		}

		[Test]
		public void ShouldSetTitle()
		{
			const string title = "the title..";
			Assert.That(_target.CreateLayoutBaseViewModel(title).Title,Is.EqualTo(title));
		}

		[Test]
		public void ShouldReturnNullIfNotSet()
		{
			var target = new LayoutBaseViewModelFactory(_cultureSpecificViewModelFactory, _datePickerGlobalizationViewModelFactory, new Now(),_userTimeZone);
			target.CreateLayoutBaseViewModel(string.Empty).FixedDate.HasValue.Should().Be.False();
		}

		[Test]
		public void ShoulGetCorrectUserTimezoneOffsetMinute()
		{
			var userTimezone = new FakeUserTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
			var time = new DateTime(2001,1,1,1,12,0,0,DateTimeKind.Local);
			var now = new MutableNow();
			now.Is(time);

			var target = new LayoutBaseViewModelFactory(_cultureSpecificViewModelFactory,
				_datePickerGlobalizationViewModelFactory,now,userTimezone);

			target.CreateLayoutBaseViewModel("title").UserTimezoneOffsetMinute.Should().Be.EqualTo(-300);
		}

		[Test]
		public void ShouldCorrectDayLightSavingTimeUserTimezonOffsetMinute()
		{
			var userTimezone = new FakeUserTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var time = new DateTime(2016,1,1,1,12,0,0,DateTimeKind.Local);
			var now = new MutableNow();
			now.Is(time);

			var target = new LayoutBaseViewModelFactory(_cultureSpecificViewModelFactory,
				_datePickerGlobalizationViewModelFactory,now,userTimezone);

			var result = target.CreateLayoutBaseViewModel("title");

			result.HasDayLightSaving.Should().Be.True();
			result.DayLightSavingStart.Should().Be("2016-03-27T00:00:00Z");
			result.DayLightSavingEnd.Should().Be("2016-10-29T23:59:59Z");
			result.DayLightSavingAdjustmentInMinute.Should().Be(60);
		}
	}

}