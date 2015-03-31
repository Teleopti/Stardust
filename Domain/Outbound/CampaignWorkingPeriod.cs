using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

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


