using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Wfm.Adherence.Historical.AdjustAdherenceToNeutral;
using Teleopti.Wfm.Adherence.Historical.ApprovePeriodAsInAdherence;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class AdjustedAdherenceSpec
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}

	public class AdjustedAdherenceSetup : IUserDataSetup<AdjustedAdherenceSpec>
	{
		private readonly AdjustAdherenceToNeutral _adjustedAsNeutral;

		public AdjustedAdherenceSetup(AdjustAdherenceToNeutral adjustedAsNeutral)
		{
			_adjustedAsNeutral = adjustedAsNeutral;
		}

		public void Apply(AdjustedAdherenceSpec spec, IPerson person, CultureInfo cultureInfo)
		{
		}
	}
}