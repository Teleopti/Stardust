using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Rta.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ApprovedPeriodSpec
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}

	public class ApprovedPeriodSetup : IUserDataSetup<ApprovedPeriodSpec>
	{
		private readonly IApprovedPeriodsPersister _persister;

		public ApprovedPeriodSetup(IApprovedPeriodsPersister persister)
		{
			_persister = persister;
		}

		public void Apply(ApprovedPeriodSpec spec, IPerson person, CultureInfo cultureInfo)
		{
			_persister
				.Persist(new ApprovedPeriod
				{
					PersonId = person.Id.Value,
					StartTime = spec.StartTime,
					EndTime = spec.EndTime
				});
		}
	}
}