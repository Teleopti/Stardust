﻿using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class RemoveDayOffFormData
	{
		public DateOnly Date { get; set; }
		public Guid[] PersonIds { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; internal set; }
	}
}