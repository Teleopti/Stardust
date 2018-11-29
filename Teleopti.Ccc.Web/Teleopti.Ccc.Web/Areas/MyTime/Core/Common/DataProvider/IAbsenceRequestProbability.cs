

using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IAbsenceRequestProbability
	{
		DateOnly Date { get; set; }
		string CssClass { get; set; }
		string Text { get; set; }
		bool Availability { get; set; }
	}
}