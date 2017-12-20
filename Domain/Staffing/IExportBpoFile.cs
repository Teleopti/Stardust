using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface IExportBpoFile
	{
		string ExportDemand(ISkill skill, DateOnlyPeriod period, IFormatProvider formatProvider, string seperator = ",",
			string dateTimeFormat = "yyyyMMdd HH:mm");
	}
}