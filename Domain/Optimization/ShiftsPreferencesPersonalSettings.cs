using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[Serializable]
	public class ShiftsPreferencesPersonalSettings : SettingValue
	{
		private bool _keepShiftCategories;
		private bool _keepStartTimes;
        private bool _keepEndTimes;
		private bool _keepShifts;
	    private IList<Guid> _selectedActivities;
		private double _keepShiftsValue;
	    private TimePeriod _selectedTimePeriod;
	    private bool _alterBetween;
		
        public ShiftsPreferencesPersonalSettings()
		{
			SetDefaultValues();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void MapTo(IShiftPreferences target,IEnumerable< IActivity > activityList )
		{
			target.KeepShiftCategories = _keepShiftCategories;
            target.KeepStartTimes = _keepStartTimes;
            target.KeepEndTimes = _keepEndTimes;
			target.KeepShifts = _keepShifts;
            if (_selectedActivities== null )
                _selectedActivities = new List<Guid>();

            if (target.SelectedActivities == null)
                target.SelectedActivities = new List<IActivity>();

            if (activityList!=null)
            {
                foreach (var activity in activityList)
                {
                    if(_selectedActivities.Contains(activity.Id.Value) )
                    {
                        if(!target.SelectedActivities.Contains(activity))
                            target.SelectedActivities.Add(activity);
                    }
                }
            }
			target.KeepShiftsValue = _keepShiftsValue;
		    target.SelectedTimePeriod = _selectedTimePeriod;
		    target.AlterBetween = _alterBetween;
		}

        public void MapFrom(IShiftPreferences source)
		{
            if (source != null)
            {
                _keepShiftCategories = source.KeepShiftCategories;
                _keepStartTimes = source.KeepStartTimes;
                _keepEndTimes = source.KeepEndTimes;
                _keepShifts = source.KeepShifts;
                if( source.SelectedActivities!= null)
                {
                    IList<Guid> guidList = new List<Guid>();
                    foreach (var activity in source.SelectedActivities)
                    {
                        guidList.Add(activity.Id.Value);
                    }
                    _selectedActivities = guidList;
                }
               
                _keepShiftsValue = source.KeepShiftsValue;
                _selectedTimePeriod = source.SelectedTimePeriod;
                _alterBetween = source.AlterBetween;
            }
		}

		private void SetDefaultValues()
		{
			_keepShiftsValue = 0.8d;
		}
	}
}
