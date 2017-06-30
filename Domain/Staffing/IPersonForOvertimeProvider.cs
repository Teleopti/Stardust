using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface IPersonForOvertimeProvider
	{
		IList<SuggestedPersonsModel> Persons(IList<Guid> skillIds, DateTime startDateTime, DateTime endDateTime, Guid multiplikator, int numToReturn);
	}

	public class SuggestedPersonsModel
	{
		public Guid PersonId { get; set; }
		public DateTime End { get; set; }
		public int TimeToAdd { get; set; }
	}
}