﻿using System;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class AgentViewModel
	{
		public Guid PersonId { get; set; }
		public string Name { get; set; }
		public string TeamId { get; set; }
		public string TeamName { get; set; }
		public string SiteId { get; set; }
		public string SiteName { get; set; }
		public double TimeZoneOffsetMinutes { get; set; }
	}

	public class AgentStateViewModel
	{
		public Guid PersonId { get; set; }
		public string State { get; set; }
		public DateTime? StateStart { get; set; }
		public string Activity { get; set; }
		public string NextActivity { get; set; }
		public DateTime? NextActivityStartTime { get; set; }
		public string Alarm { get; set; }
		public DateTime? AlarmStart { get; set; }
		public string AlarmColor { get; set; }
	}

}