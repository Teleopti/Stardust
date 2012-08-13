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
       
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void MapTo(ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories)
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
      	}
	}
}