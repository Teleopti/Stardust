﻿using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class MoveActivityCommand
	{
		public Guid AgentId { get; set; }
		public DateOnly ScheduleDate { get; set; }
		public Guid ActivityId { get; set; }
		public DateTime NewStartTime { get; set; }
		public DateTime OldStartTime { get; set; }
		public int OldProjectionLayerLength { get; set; }
	}
}