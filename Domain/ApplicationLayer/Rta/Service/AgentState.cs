using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStatePrepare
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid? TeamId { get; set; }
		public Guid? SiteId { get; set; }
		public IEnumerable<ExternalLogon> ExternalLogons { get; set; }
	}

	public class ExternalLogon
	{
		public int DataSourceId { get; set; }
		public string UserCode { get; set; }
	}

	public class AgentStateFound : AgentState
	{
		public int DataSourceId { get; set; }
		public string UserCode { get; set; }
	}

	public class AgentState
	{
		public DateTime? BatchId { get; set; }
		public Guid PlatformTypeId { get; set; }
		public string SourceId { get; set; }

		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid? TeamId { get; set; }
		public Guid? SiteId { get; set; }
		public DateTime? ReceivedTime { get; set; }
		public string StateCode { get; set; }
		public Guid? StateGroupId { get; set; }
		public DateTime? StateStartTime { get; set; }

		public Guid? ActivityId { get; set; }

		public Guid? RuleId { get; set; }
		public DateTime? RuleStartTime { get; set; }
		public EventAdherence? Adherence { get; set; }

		public DateTime? AlarmStartTime { get; set; }

		public int? TimeWindowCheckSum { get; set; }

		public IEnumerable<ScheduledActivity> Schedule { get; set; }
		public DateTime? NextCheck { get; set; }

	}

	public static class AgentStateExtensions
	{
		public static Guid PlatformTypeId(this AgentState stored)
		{
			return stored?.PlatformTypeId ?? default(Guid);
		}

		public static DateTime ReceivedTime(this AgentState stored)
		{
			return stored?.ReceivedTime ?? DateTime.MinValue;
		}
	}
}
