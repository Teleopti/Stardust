using System;

namespace Teleopti.Wfm.Administration.Models.Stardust
{
	public class LogOnModel
	{
		public string Tenant;
		public int Days;
	}

	public class SkillForecastCalculationModel
	{
		public string Tenant;
		public DateTime StartDate;
		public DateTime EndDate;
	}
}