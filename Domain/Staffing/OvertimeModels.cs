using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Interfaces.Domain;

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
		public OvertimePreferences OvertimePreferences { get; set; }
		public  int NumberOfPersonsToTry { get; set; }
	}

	public class GetOvertimeSuggestionModel
	{
		public IList<Guid> SkillIds { get; set; }
		public DateTime[] TimeSerie { get; set; }
		public OvertimePreferencesModel OvertimePreferences { get; set; }
		public int NumberOfPersonsToTry { get; set; }
	}

	public class OvertimePreferencesModel
	{
		public Guid Compensation { get; set; }
		public int MinTinutesToAdd { get; set; }
		public int MaxMinutesToAdd { get; set; }
		public bool AllowBreakMaxWorkPerWeek { get; set; }
		public bool AllowBreakNightlyRest { get; set; }
		public bool AllowBreakWeeklyRest { get; set; }
	}
	public class OverTimeModel
	{
		public Guid ActivityId { get; set; }
		public Guid PersonId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
	}

	public class CompensationModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}
