

using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public class DistributionReportData
	{
		private readonly IDictionary<IShiftCategory, DistributionReportDataValues> _distributionDictionary =
			new Dictionary<IShiftCategory, DistributionReportDataValues>();

		public IDictionary<IShiftCategory, DistributionReportDataValues> DistributionDictionary
		{
			get { return _distributionDictionary; }
		}
	}

	public class DistributionReportDataValues
	{
		public double Agent { get; set; }
		public double Team { get; set; }
		public double All { get; set; }
	}
}