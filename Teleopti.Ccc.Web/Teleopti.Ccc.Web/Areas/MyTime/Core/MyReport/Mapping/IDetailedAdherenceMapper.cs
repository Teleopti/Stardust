using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping
{
	public interface IDetailedAdherenceMapper
	{
		DetailedAdherenceViewModel Map(ICollection<DetailedAdherenceForDayResult> dataModel);
	}
}