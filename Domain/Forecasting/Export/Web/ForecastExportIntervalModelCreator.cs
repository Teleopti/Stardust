using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export.Web
{
	public static class ForecastExportIntervalModelCreator
	{
		public static IList<ForecastExportIntervalModel> Load(List<ISkillDay> skillId)
		{
			return new List<ForecastExportIntervalModel>();
		}
	}
}