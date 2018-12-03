using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class ShiftsPreferencesPersonalSettings : SettingValue
	{
		private bool _keepShiftCategories;
		private bool _keepStartTimes;
        private bool _keepEndTimes;
		private IList<Guid> _selectedActivities = new List<Guid>();
	    private TimePeriod _selectedTimePeriod;
	    private bool _alterBetween;
		private bool _keepActivityLength;
		private Guid? _selectedActivityToKeepLenghtOn;
		
		public void MapTo(ShiftPreferences target,IEnumerable< IActivity > activityList )
		{
			target.KeepShiftCategories = _keepShiftCategories;
            target.KeepStartTimes = _keepStartTimes;
            target.KeepEndTimes = _keepEndTimes;

            if (target.SelectedActivities == null)
                target.SelectedActivities = new List<IActivity>();

			target.KeepActivityLength = _keepActivityLength;
		    target.SelectedTimePeriod = _selectedTimePeriod;
		    target.AlterBetween = _alterBetween;

			if (activityList == null) return;

			foreach (var activity in activityList)
			{
				if (_selectedActivities.Contains(activity.Id.GetValueOrDefault()))
				{
					if (!target.SelectedActivities.Contains(activity))
						target.SelectedActivities.Add(activity);
				}
				if (_selectedActivityToKeepLenghtOn == activity.Id.GetValueOrDefault())
				{
					target.ActivityToKeepLengthOn = activity;
				}
			}
		}

        public void MapFrom(ShiftPreferences source)
		{
	        if (source == null) return;

	        _keepShiftCategories = source.KeepShiftCategories;
	        _keepStartTimes = source.KeepStartTimes;
	        _keepEndTimes = source.KeepEndTimes;
	        if( source.SelectedActivities!= null)
	        {
		        _selectedActivities = source.SelectedActivities.Select(activity => activity.Id.GetValueOrDefault()).ToList();
	        }
               
	        _selectedTimePeriod = source.SelectedTimePeriod;
	        _alterBetween = source.AlterBetween;
	        _keepActivityLength = source.KeepActivityLength;
	        _selectedActivityToKeepLenghtOn = null;
	        if (source.ActivityToKeepLengthOn != null)
	        {
		        _selectedActivityToKeepLenghtOn = source.ActivityToKeepLengthOn.Id;
	        }
		}
	}
}
