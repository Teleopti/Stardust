using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.Models
{
	public class CampaignViewModel
	{
		public string Id;
		public string Name;
		public IEnumerable<SkillViewModel> Skills;  
		public int CallListLen;
		public int TargetRate;
		public int ConnectRate;
		public int RightPartyConnectRate;
		public int ConnectAverageHandlingTime;
		public int RightPartyAverageHandlingTime;
		public int UnproductiveTime;
		public DateOnly? StartDate;
		public DateOnly? EndDate;
		public CampaignStatus CampaignStatus;
		public IEnumerable<CampaignWorkingPeriodViewModel> CampaignWorkingPeriods;		
	}

	public class CampaignWorkingPeriodViewModel
	{
		public Guid? Id;
		public TimePeriod WorkingPeriod;
		public IEnumerable<CampaignWorkingPeriodAssignmentViewModel> WorkingPeroidAssignments;
	}

	public class CampaignWorkingPeriodAssignmentViewModel
	{
		public Guid? Id;
		public DayOfWeek WeekDay;
	}

	public class SkillViewModel
	{
		public Guid? Id;
		public string SkillName;
		public bool IsSelected;
	}
}