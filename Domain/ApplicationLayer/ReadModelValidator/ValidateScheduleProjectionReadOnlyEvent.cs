using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ValidateScheduleProjectionReadOnlyEvent : EventWithInfrastructureContext
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}
