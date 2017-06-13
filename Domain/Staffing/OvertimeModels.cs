using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class OverTimeStaffingSuggestionModel
	{
		public IList<SkillStaffingInterval> SkillStaffingIntervals { get; set; }
		public IList<OverTimeModel> OverTimeModels { get; set; }
	}


	public class OverTimeSuggestionResultModel
	{
		public StaffingDataSeries DataSeries { get; set; }

		public bool StaffingHasData { get; set; }
		public IList<OverTimeModel> OverTimeModels { get; set; }
	}

	public class OverTimeSuggestionModel
	{
		public IList<Guid> SkillIds { get; set; }
		public DateTime[] TimeSerie { get; set; }
	}

	public class OverTimeModel
	{
		public Guid ActivityId { get; set; }
		public Guid PersonId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
	}
}
