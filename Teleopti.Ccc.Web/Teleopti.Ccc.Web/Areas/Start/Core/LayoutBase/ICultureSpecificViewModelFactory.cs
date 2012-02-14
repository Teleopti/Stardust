using Teleopti.Ccc.Web.Areas.Start.Models.LayoutBase;

namespace Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase
{
	public interface ICultureSpecificViewModelFactory 
	{
		CultureSpecificViewModel CreateCutureSpecificViewModel();
	}
}