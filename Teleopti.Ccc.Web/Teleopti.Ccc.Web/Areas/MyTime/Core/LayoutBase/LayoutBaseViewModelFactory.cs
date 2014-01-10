using System;
using Teleopti.Ccc.Domain.Common;
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
		public LayoutBaseViewModel CreateLayoutBaseViewModel(string title)
		{
			var cultureSpecificViewModel = _cultureSpecificViewModelFactory.CreateCutureSpecificViewModel();
			var datePickerGlobalizationViewModel =
				_datePickerGlobalizationViewModelFactory.CreateDatePickerGlobalizationViewModel();
			DateTime? fixedDate = null;

			if (_now is IMutateNow && (_now as IMutateNow).IsMutated())
				fixedDate = _now.UtcDateTime();
			return new LayoutBaseViewModel
				{
					CultureSpecific = cultureSpecificViewModel,
					DatePickerGlobalization = datePickerGlobalizationViewModel,
					Footer = string.Empty,
					Title = title,
					FixedDate = fixedDate
				};
		}
	}
}