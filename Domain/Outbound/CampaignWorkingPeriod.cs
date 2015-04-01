using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound
{
	public class CampaignWorkingPeriod : AggregateEntity
	{
		private TimePeriod _timePeriod;
		private IEnumerable<CampaignWorkingPeriodAssignment> _campaignWorkingPeriodAssignments;

		public virtual TimePeriod TimePeriod
		{
			get { return _timePeriod; }
			set { _timePeriod = value; }
		}

		public virtual IEnumerable<CampaignWorkingPeriodAssignment> CampaignWorkingPeriodAssignments
		{
			get { return _campaignWorkingPeriodAssignments; }
			set { _campaignWorkingPeriodAssignments = value; }
		}
	}
}


