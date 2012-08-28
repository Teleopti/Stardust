using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.WebTest.Core.Portal
{
	public class LayoutBaseViewModelFactoryTest
	{
		private ILayoutBaseViewModelFactory _target;
		private MockRepository _mocks;
		private ICultureSpecificViewModelFactory _cultureSpecificViewModelFactory;
		private IDatePickerGlobalizationViewModelFactory _datePickerGlobalizationViewModelFactory;


		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_cultureSpecificViewModelFactory = _mocks.DynamicMock<ICultureSpecificViewModelFactory>();
			_datePickerGlobalizationViewModelFactory = _mocks.DynamicMock<IDatePickerGlobalizationViewModelFactory>();
			_target = new LayoutBaseViewModelFactory(_cultureSpecificViewModelFactory, _datePickerGlobalizationViewModelFactory, new Now());
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
				var result = _target.CreateLayoutBaseViewModel();

				result.Should().Not.Be.Null();
				result.CultureSpecific.Should().Be.SameInstanceAs(cultureSpecificViewModel);
				result.DatePickerGlobalization.Should().Be.SameInstanceAs(datePickerGlobalizationViewModel);
			}
		}

		[Test]
		public void ShouldSetTime()
		{
			var year1970 = new DateTime(1970, 1, 1,0,0,0,DateTimeKind.Utc);
			var today = new DateTime(2001, 1, 1,1,12,0,0,DateTimeKind.Utc);

			var expected = today.Subtract(year1970).TotalMilliseconds;
			
			var time = new Now();
			((IModifyNow)time).SetNow(today);
			var target = new LayoutBaseViewModelFactory(_cultureSpecificViewModelFactory, _datePickerGlobalizationViewModelFactory, time);
			
			target.CreateLayoutBaseViewModel().ExplicitlySetMilliSecondsFromYear1970.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldSReturnZeroIfNotSet()
		{
			var target = new LayoutBaseViewModelFactory(_cultureSpecificViewModelFactory, _datePickerGlobalizationViewModelFactory, new Now());
			target.CreateLayoutBaseViewModel().ExplicitlySetMilliSecondsFromYear1970.Should().Be.EqualTo(0);
		}
	}
}