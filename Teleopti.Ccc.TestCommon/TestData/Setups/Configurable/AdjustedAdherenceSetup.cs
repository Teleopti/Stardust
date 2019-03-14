using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Wfm.Adherence.Historical.Adjustment;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class AdjustedAdherenceSpec
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}

	public class AdjustedAdherenceSetup : IDataSetup<AdjustedAdherenceSpec>
	{
		private readonly Adjustment _adjustedAsNeutral;

		public AdjustedAdherenceSetup(Adjustment adjustedAsNeutral)
		{
			_adjustedAsNeutral = adjustedAsNeutral;
		}

		public void Apply(AdjustedAdherenceSpec spec)
		{
			_adjustedAsNeutral.AdjustToNeutral(
				new PeriodToAdjust
				{
					StartTime = spec.StartTime,
					EndTime = spec.EndTime						
				});
		}
	}
}