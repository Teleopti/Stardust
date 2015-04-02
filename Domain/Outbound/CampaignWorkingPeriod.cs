using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound
{
	public class CampaignWorkingPeriod : AggregateEntity, IEquatable<CampaignWorkingPeriod>
	{
		private TimePeriod _timePeriod;
		private ISet<CampaignWorkingPeriodAssignment> _campaignWorkingPeriodAssignments;

		public CampaignWorkingPeriod()
		{
			_campaignWorkingPeriodAssignments = new HashSet<CampaignWorkingPeriodAssignment>();
		}

		public virtual TimePeriod TimePeriod
		{
			get { return _timePeriod; }
			set { _timePeriod = value; }
		}

		public virtual ISet<CampaignWorkingPeriodAssignment> CampaignWorkingPeriodAssignments
		{
			get { return _campaignWorkingPeriodAssignments; }
			set { _campaignWorkingPeriodAssignments = value; }
		}

		public virtual void AddAssignment(CampaignWorkingPeriodAssignment assignment)
		{
			// Check assignment already taken
			if (_campaignWorkingPeriodAssignments.Any(_assignment =>
			{
				if (_assignment == null) throw new ArgumentNullException("_assignment");
				return _assignment.WeekdayIndex == assignment.WeekdayIndex;
			}))
			{
				throw new ArgumentException("The specified weekday has already been taken.");
			}

			assignment.SetParent(this);
			_campaignWorkingPeriodAssignments.Add(assignment);
		}

		public virtual void RemoveAssignment(CampaignWorkingPeriodAssignment assignment)
		{
			if (_campaignWorkingPeriodAssignments.Contains(assignment))
			{
				_campaignWorkingPeriodAssignments.Remove(assignment);
			}
		}

		
		public virtual bool Equals(CampaignWorkingPeriod other)
		{
			if (other.Id.HasValue && Id.HasValue)
			{
				return other.Id.Value == Id.Value;
			}
			return false;
		}
	}
}


