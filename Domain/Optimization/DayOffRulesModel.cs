﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffRulesModel
	{
		public DayOffRulesModel()
		{
			Filters = new List<FilterModel>();
		}

		public int MinDayOffsPerWeek { get; set; }
		public int MaxDayOffsPerWeek { get; set; }
		public int MinConsecutiveWorkdays { get; set; }
		public int MaxConsecutiveWorkdays { get; set; }
		public int MinConsecutiveDayOffs { get; set; }
		public int MaxConsecutiveDayOffs { get; set; }
		public Guid Id { get; set; }
		public bool Default { get; set; }
		public IList<FilterModel> Filters { get; set; }
		public string Name { get; set; }
		public Guid? PlanningGroupId { get; set; }
		public BlockFinderType BlockFinderType { get; set; }
		public bool BlockSameStartTime { get; set; }
		public bool BlockSameShiftCategory { get; set; }
		public bool BlockSameShift { get; set; }
	}
}