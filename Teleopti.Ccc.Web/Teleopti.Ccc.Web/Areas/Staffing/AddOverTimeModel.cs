using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Staffing
{
	public class AddOverTimeModel
	{
		public DateTimePeriod Period;
		public IList<Guid> Skills;
	}
}