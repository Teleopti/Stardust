﻿using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddOvertimeActivityCommand:ITrackableCommand, IErrorAttachedCommand
	{
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }
		public DateTimePeriod Period { get; set; }
		public Guid ActivityId { get; set; }
		public Guid MultiplicatorDefinitionSetId { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public IList<string> ErrorMessages { get; set; }
	}
}
