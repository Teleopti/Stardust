using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class SchedulingOptionsAdvancedPersonalSetting : SettingValue
	{
		private bool _useMinStaff = true;
		private bool _useMaxStaff = true;
		private bool _useMaxSeats = true;
		private bool _doNotBreakMaxSeats;
		private Guid? _shiftCategoryId;
		private double _fairnessValue;
		private string _fairnessGroupPageKey;
		private int _resourceCalculateFrequency = 1;
		private int _screenRefreshRate = 10;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void MapTo(ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories, IList<IGroupPage> groupPages)
		{
			schedulingOptions.UseMinimumPersons = _useMinStaff;
			schedulingOptions.UseMaximumPersons = _useMaxStaff;
			schedulingOptions.UseMaxSeats = _useMaxSeats;
			schedulingOptions.DoNotBreakMaxSeats = _doNotBreakMaxSeats;

			if(_shiftCategoryId.HasValue)
			{
				foreach (var shiftCategory in shiftCategories)
				{
					if (shiftCategory.Id == _shiftCategoryId)
					{
						schedulingOptions.ShiftCategory = shiftCategory;
					}
				}
			}
			
			schedulingOptions.Fairness = new Percent(_fairnessValue);

			foreach (var groupPage in groupPages)
			{
				if (_fairnessGroupPageKey == groupPage.Key)
					schedulingOptions.GroupPageForShiftCategoryFairness = groupPage;
			}

			if (_resourceCalculateFrequency < 1 || _resourceCalculateFrequency > 10)
				_resourceCalculateFrequency = 1;
			schedulingOptions.ResourceCalculateFrequency = _resourceCalculateFrequency;

			if (_screenRefreshRate < 1 || _screenRefreshRate > 999)
				_screenRefreshRate = 10;
			schedulingOptions.RefreshRate = _screenRefreshRate;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void MapFrom(ISchedulingOptions schedulingOptions)
		{
			_useMinStaff = schedulingOptions.UseMinimumPersons;
			_useMaxStaff = schedulingOptions.UseMaximumPersons;
			_useMaxSeats = schedulingOptions.UseMaxSeats;
			_doNotBreakMaxSeats = schedulingOptions.DoNotBreakMaxSeats;
			if (schedulingOptions.ShiftCategory != null)
				_shiftCategoryId = schedulingOptions.ShiftCategory.Id;
			else
			{
				_shiftCategoryId = null;
			}
			_fairnessValue = schedulingOptions.Fairness.Value;
			_fairnessGroupPageKey = schedulingOptions.GroupPageForShiftCategoryFairness.Key;
			_resourceCalculateFrequency = schedulingOptions.ResourceCalculateFrequency;
			_screenRefreshRate = schedulingOptions.RefreshRate;
		}
	}
}