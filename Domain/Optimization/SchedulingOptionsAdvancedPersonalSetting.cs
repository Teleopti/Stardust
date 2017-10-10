﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class SchedulingOptionsAdvancedPersonalSetting : SettingValue
	{
		private bool _useMinStaff = true;
		private bool _useMaxStaff = true;
		private bool _useAverageShiftLengths = true;
		private Guid? _shiftCategoryId;
		private int _refreshRate;
       
		public void MapTo(SchedulingOptions schedulingOptions, IEnumerable<IShiftCategory> shiftCategories)
		{
			schedulingOptions.UseMinimumStaffing = _useMinStaff;
			schedulingOptions.UseMaximumStaffing = _useMaxStaff;
			schedulingOptions.UseAverageShiftLengths = _useAverageShiftLengths;
			schedulingOptions.RefreshRate = _refreshRate < 1? 10: _refreshRate;

			if (!_shiftCategoryId.HasValue) return;
			schedulingOptions.ShiftCategory =
				shiftCategories.FirstOrDefault(shiftCategory => shiftCategory.Id == _shiftCategoryId);
		}

		public void MapFrom(SchedulingOptions schedulingOptions)
		{
			_useMinStaff = schedulingOptions.UseMinimumStaffing;
			_useMaxStaff = schedulingOptions.UseMaximumStaffing;
			_useAverageShiftLengths = schedulingOptions.UseAverageShiftLengths;
			_refreshRate = schedulingOptions.RefreshRate;
			_shiftCategoryId = schedulingOptions.ShiftCategory != null ? schedulingOptions.ShiftCategory.Id : null;
      	}
	}
}