using System.Drawing;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping
{
	public class CommonViewModelMapper
	{
		public StyleClassViewModel Map(Color s)
		{
			return new StyleClassViewModel
			{
				Name = s.ToStyleClass(),
				ColorHex = s.ToHtml(),
				RgbColor = s.ToCSV()
			};
		}
	}
}