﻿using System;

namespace Teleopti.Wfm.Api.Query
{
	public class ScheduleByPersonIdDto : IQueryDto
	{
		public Guid PersonId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}