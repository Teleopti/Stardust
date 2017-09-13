using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ValidateReadModelsEvent : StardustJobInfo
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public ValidateReadModelType Targets { get; set; }
		public bool TriggerFix { get; set; }
		public bool Reinitialize { get; set; }
	}
}
