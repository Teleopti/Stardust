

using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AbsenceRequestProbability : IAbsenceRequestProbability
	{
		public DateOnly Date { get; set; }
		public string CssClass { get; set; }
		public string Text { get; set; }
		public bool Availability { get; set; }
	}
}