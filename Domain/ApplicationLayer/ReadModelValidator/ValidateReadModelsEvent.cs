﻿using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ValidateReadModelsEvent : EventWithInfrastructureContext
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public ValidateReadModelType Targets { get; set; }
	}
}
