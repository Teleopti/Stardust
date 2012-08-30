using System;
using Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase
{
	public class LayoutBaseViewModelFactory : ILayoutBaseViewModelFactory
	{
		private readonly ICultureSpecificViewModelFactory _cultureSpecificViewModelFactory;
		private readonly IDatePickerGlobalizationViewModelFactory _datePickerGlobalizationViewModelFactory;
		private readonly INow _now;

		public LayoutBaseViewModelFactory(ICultureSpecificViewModelFactory cultureSpecificViewModelFactory, 
													IDatePickerGlobalizationViewModelFactory datePickerGlobalizationViewModelFactory, 
													INow now)
		{
			_cultureSpecificViewModelFactory = cultureSpecificViewModelFactory;
			_datePickerGlobalizationViewModelFactory = datePickerGlobalizationViewModelFactory;
			_now = now;
		}

		// TODO: JonasN, Texts
		public LayoutBaseViewModel CreateLayoutBaseViewModel()
		{
			var cultureSpecificViewModel = _cultureSpecificViewModelFactory.CreateCutureSpecificViewModel();
			var datePickerGlobalizationViewModel =
				_datePickerGlobalizationViewModelFactory.CreateDatePickerGlobalizationViewModel();
			double milliseconds = 0;

			if (_now.IsExplicitlySet())
			{
				milliseconds = _now.UtcDateTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
			}
			return new LayoutBaseViewModel
			       	{
			       		CultureSpecific = cultureSpecificViewModel,
			       		DatePickerGlobalization = datePickerGlobalizationViewModel,
			       		Footer = string.Empty,
			       		Title = "MyTime",
							ExplicitlySetMilliSecondsFromYear1970 = milliseconds
			       	};
		}
	}
}