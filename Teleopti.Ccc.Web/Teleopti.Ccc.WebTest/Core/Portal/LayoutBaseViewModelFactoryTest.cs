using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Interfaces.Domain;

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
			_target = new LayoutBaseViewModelFactory(_cultureSpecificViewModelFactory, _datePickerGlobalizationViewModelFactory, new Now(null));
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
			var date = new DateTime(2001, 1, 1,1,12,0,0,DateTimeKind.Utc);
			var nowComponent = MockRepository.GenerateMock<INow>();

			nowComponent.Expect(c => c.IsExplicitlySet()).Return(true);
			nowComponent.Expect(c => c.LocalDateTime()).Return(date);

			var target = new LayoutBaseViewModelFactory(_cultureSpecificViewModelFactory, _datePickerGlobalizationViewModelFactory, nowComponent);
			
			target.CreateLayoutBaseViewModel().FixedDate.Should().Be.EqualTo(date);
		}

		[Test]
		public void ShouldReturnNullIfNotSet()
		{
			var target = new LayoutBaseViewModelFactory(_cultureSpecificViewModelFactory, _datePickerGlobalizationViewModelFactory, new Now(null));
			target.CreateLayoutBaseViewModel().FixedDate.HasValue.Should().Be.False();
		}
	}
}