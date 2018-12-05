using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Wfm.Adherence.Historical.ApprovePeriodAsInAdherence;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ApprovedPeriodSpec
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}

	public class ApprovedPeriodSetup : IUserDataSetup<ApprovedPeriodSpec>
	{
		private readonly ApprovePeriodAsInAdherence _approvedAsInAdherence;

		public ApprovedPeriodSetup(ApprovePeriodAsInAdherence approvedAsInAdherence)
		{
			_approvedAsInAdherence = approvedAsInAdherence;
		}

		public void Apply(ApprovedPeriodSpec spec, IPerson person, CultureInfo cultureInfo)
		{
			_approvedAsInAdherence.Approve(new ApprovedPeriod
			{
				PersonId = person.Id.Value,
				StartTime = spec.StartTime,
				EndTime = spec.EndTime
			});
		}
	}
}