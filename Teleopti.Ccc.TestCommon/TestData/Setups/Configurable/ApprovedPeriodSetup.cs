using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Wfm.Adherence.Historical.Approval;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ApprovedPeriodSpec
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}

	public class ApprovedPeriodSetup : IUserDataSetup<ApprovedPeriodSpec>
	{
		private readonly Approval _approvedAsInAdherence;

		public ApprovedPeriodSetup(Approval approvedAsInAdherence)
		{
			_approvedAsInAdherence = approvedAsInAdherence;
		}

		public void Apply(ApprovedPeriodSpec spec, IPerson person, CultureInfo cultureInfo)
		{
			_approvedAsInAdherence.ApproveAsInAdherence(new PeriodToApprove
			{
				PersonId = person.Id.Value,
				StartTime = spec.StartTime,
				EndTime = spec.EndTime
			});
		}
	}
}