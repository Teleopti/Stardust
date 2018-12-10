

using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class MustHaveInput
	{
		public DateOnly Date { get; set; }
		public bool MustHave { get; set; }
	}
}