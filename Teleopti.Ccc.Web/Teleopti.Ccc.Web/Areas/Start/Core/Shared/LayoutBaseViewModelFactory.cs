using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.Start.Models.LayoutBase;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Shared
{
	public class LayoutBaseViewModelFactory : ILayoutBaseViewModelFactory
	{
		private readonly ICultureSpecificViewModelFactory _cultureSpecificViewModelFactory;

		public LayoutBaseViewModelFactory(ICultureSpecificViewModelFactory cultureSpecificViewModelFactory) {
			_cultureSpecificViewModelFactory = cultureSpecificViewModelFactory;
		}

		public LayoutBaseViewModel CreateLayoutBaseViewModel()
		{
			return new LayoutBaseViewModel
			       	{
						CultureSpecific = _cultureSpecificViewModelFactory.CreateCutureSpecificViewModel(),
						Footer = "",
						Title = Resources.OpenTeleoptiCCC
					};
		}
	}
}