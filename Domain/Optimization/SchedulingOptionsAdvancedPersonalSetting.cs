using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class SchedulingOptionsAdvancedPersonalSetting : SettingValue
	{
		private bool _useMinStaff = true;
		private bool _useMaxStaff = true;
		private bool _useAverageShiftLengths = true;
		private MaxSeatsFeatureOptions _maxSeatsFeatureOptions;
		private Guid? _shiftCategoryId;
       
		public void MapTo(ISchedulingOptions schedulingOptions, IEnumerable<IShiftCategory> shiftCategories)
		{
			schedulingOptions.UseMinimumPersons = _useMinStaff;
			schedulingOptions.UseMaximumPersons = _useMaxStaff;
			schedulingOptions.UserOptionMaxSeatsFeature = _maxSeatsFeatureOptions;
			schedulingOptions.UseAverageShiftLengths = _useAverageShiftLengths;

			if (!_shiftCategoryId.HasValue) return;
			schedulingOptions.ShiftCategory =
				shiftCategories.FirstOrDefault(shiftCategory => shiftCategory.Id == _shiftCategoryId);
		}

		public void MapFrom(ISchedulingOptions schedulingOptions)
		{
			_useMinStaff = schedulingOptions.UseMinimumPersons;
			_useMaxStaff = schedulingOptions.UseMaximumPersons;
			_maxSeatsFeatureOptions = schedulingOptions.UserOptionMaxSeatsFeature;
			_useAverageShiftLengths = schedulingOptions.UseAverageShiftLengths;
			_shiftCategoryId = schedulingOptions.ShiftCategory != null ? schedulingOptions.ShiftCategory.Id : null;
      	}
	}
}