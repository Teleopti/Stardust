using System;

namespace Teleopti.Ccc.Domain.Forecasting.Models
{
	public class ForecastResultModel
	{
		public DateTime date;
		public double vc;
		public double vtc;
		public double vtt;
		public double vttt;
		public double vacw;
		public double vtacw;
		public int vcombo;
		public int voverride;
		public int vcampaign;
	}
}