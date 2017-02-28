using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
    public class SchedulePeriodModel : GridViewModelBase<ISchedulePeriod>, ISchedulePeriodModel
    {
        private readonly IPerson _containedEntity;
        private ISchedulePeriod _currentSchedulePeriod;
        private readonly CommonNameDescriptionSetting _commonNameDescription;

	    public SchedulePeriodModel(DateOnly selectedDate, IPerson person, CommonNameDescriptionSetting commonNameDescription)
        {
            _containedEntity = person;
            _currentSchedulePeriod = _containedEntity.SchedulePeriod(selectedDate);
            _commonNameDescription = commonNameDescription;
        }

		public IPerson Parent
        {
            get { return _containedEntity; }
        }

        public string FullName
        {
            get {
	            return _commonNameDescription == null ? _containedEntity.Name.ToString() : _commonNameDescription.BuildCommonNameDescription(_containedEntity);
            }
        }

        public int Number
        {
            get
            {
                if (_currentSchedulePeriod == null) return -1;
                return _currentSchedulePeriod.Number;
            }
            set { if (_currentSchedulePeriod != null) _currentSchedulePeriod.Number = value; }
        }

        public DateOnly? PeriodDate
        {
            get
            {
	            return _currentSchedulePeriod == null ? (DateOnly?)null : _currentSchedulePeriod.DateFrom;
            }
	        set
            {
				if (!value.HasValue) return;
				if (_currentSchedulePeriod == null) return;
                if (value == _currentSchedulePeriod.DateFrom) return;

	            _containedEntity.ChangeSchedulePeriodStartDate(value.Value, _currentSchedulePeriod);
            }
        }

        public GridControl GridControl { get; set; }

		public SchedulePeriodType? PeriodType
		{
			get
			{
				if (_currentSchedulePeriod == null) return null;
				return _currentSchedulePeriod.PeriodType;
			}
			set
			{
				if (value.HasValue)
				{
					_currentSchedulePeriod.PeriodType = value.Value;
				}
			}
		}

        public bool ExpandState { get; set; }

        public ISchedulePeriod GetCurrentPersonPeriodByDate(DateOnly selectedDate)
        {
            _currentSchedulePeriod = _containedEntity.SchedulePeriod(selectedDate);
            return _currentSchedulePeriod;
        }

        public int? DaysOff
        {
            get
            {
                if (_currentSchedulePeriod == null) return null;
                return _currentSchedulePeriod.GetDaysOff(_currentSchedulePeriod.DateFrom);
            }
            set
            {
                if (_currentSchedulePeriod!=null && value.HasValue) 
                    _currentSchedulePeriod.SetDaysOff(value.Value);
            }
        }

        public TimeSpan AverageWorkTimePerDay
        {
            get
            {
	            return _currentSchedulePeriod == null ? TimeSpan.MinValue : _currentSchedulePeriod.AverageWorkTimePerDay;
            }
        }

		public virtual TimeSpan AverageWorkTimePerDayOverride
		{
			get
			{
                if (_currentSchedulePeriod == null) return TimeSpan.MinValue;
				return _currentSchedulePeriod.AverageWorkTimePerDayOverride;
			}
			set
			{
				if (_currentSchedulePeriod != null)
					_currentSchedulePeriod.AverageWorkTimePerDayOverride = value;
			}
		}

        public TimeSpan BalanceIn
        {
            get
            {
                if (_currentSchedulePeriod == null) return TimeSpan.MinValue;
                return _currentSchedulePeriod.BalanceIn;
            }
            set
            {
                if (_currentSchedulePeriod != null)
                    _currentSchedulePeriod.BalanceIn = value;
            }
        }

        public TimeSpan BalanceOut
        {
            get
            {
                if (_currentSchedulePeriod == null) return TimeSpan.MinValue;
                return _currentSchedulePeriod.BalanceOut;
            }
            set
            {
                if (_currentSchedulePeriod != null)
                    _currentSchedulePeriod.BalanceOut = value;
            }
        }

        public Percent Seasonality
        {
            get
            {
                if (_currentSchedulePeriod == null) return new Percent(0);
                return _currentSchedulePeriod.Seasonality;
            }
            set
            {
                if (_currentSchedulePeriod != null)
                    _currentSchedulePeriod.Seasonality = value;
            }
        }

        public TimeSpan Extra
        {
            get
            {
                if (_currentSchedulePeriod == null) return TimeSpan.MinValue;
                return _currentSchedulePeriod.Extra;
            }
            set
            {
                if (_currentSchedulePeriod != null)
                    _currentSchedulePeriod.Extra = value;
            }
        }

        public int OverrideDaysOff
        {
            get
            {
                if (_currentSchedulePeriod == null)
                    return -1;
				if (!_currentSchedulePeriod.DaysOff.HasValue)
					return -1;
				return _currentSchedulePeriod.DaysOff.Value;
            }
            set
            {
                if (_currentSchedulePeriod != null)
                    _currentSchedulePeriod.SetDaysOff(value);
            }
        }

        public int PeriodCount
        {
            get
            {
                if (_currentSchedulePeriod != null && Parent.PersonSchedulePeriodCollection.Count == 1)
                    return 0;
                return Parent.PersonSchedulePeriodCollection.Count;
            }
        }

        public bool CanGray
        {
            get
            {
                return _currentSchedulePeriod == null;
            }
        }

        public bool IsDaysOffOverride
        {
            get
            {
                SchedulePeriod currentSchedulePeriod = _currentSchedulePeriod as SchedulePeriod;
                if (currentSchedulePeriod != null)
                    return currentSchedulePeriod.IsDaysOffOverride;
                return false;
            }
        }

        public bool IsAverageWorkTimePerDayOverride
        {
            get
            {
                if (_currentSchedulePeriod != null)
                {
                    SchedulePeriod currentSchedulePeriod = _currentSchedulePeriod as SchedulePeriod;
                    if (currentSchedulePeriod != null)
                        return currentSchedulePeriod.IsAverageWorkTimePerDayOverride;
                }
                return false;
            }
        }
        
		public int MustHavePreference
        {
            get
            {
                if (_currentSchedulePeriod == null) return 0;
                return _currentSchedulePeriod.MustHavePreference;
            }
            set
            {
                if (_currentSchedulePeriod != null)
                {
                    _currentSchedulePeriod.MustHavePreference = value;
                }
            }
        }

        public ISchedulePeriod SchedulePeriod
        {
            get
            {
                return _currentSchedulePeriod;
            }
        }

        public void ResetCanBoldPropertyOfChildAdapters()
        {
            if (GridControl != null)
            {
                IList<SchedulePeriodChildModel> childAdapters = GridControl.Tag as
                    IList<SchedulePeriodChildModel>;

                if (childAdapters != null)
                {
                    for (int i = 0; i < childAdapters.Count; i++)
                    {
                        childAdapters[i].CanBold = false;
                    }
                }

                GridControl.Invalidate();
            }
        }

		public bool AdapterOrChildCanBold()
		{
			if (CanBold) return true;

			if (GridControl != null)
			{
				var childAdapters = GridControl.Tag as IList<SchedulePeriodChildModel>;

				if (childAdapters != null)
				{
					for (var i = 0; i < childAdapters.Count; i++)
					{
						if (childAdapters[i].CanBold) return true;
					}
				}
			}

			return false;
		}

		public bool IsPeriodTargetOverride
		{
			get
			{
				if (_currentSchedulePeriod != null)
				{
					SchedulePeriod currentSchedulePeriod = _currentSchedulePeriod as SchedulePeriod;
					if (currentSchedulePeriod != null)
						return currentSchedulePeriod.IsPeriodTimeOverride;
				}
				return false;
			}
		}

		public TimeSpan PeriodTime
		{
			get
			{
				if (_currentSchedulePeriod == null) return TimeSpan.MinValue;
				TimeSpan? value = _currentSchedulePeriod.PeriodTime;
				if (!value.HasValue)
					return TimeSpan.MinValue;
				return _currentSchedulePeriod.PeriodTime.Value;
			}
			set
			{
				if (_currentSchedulePeriod != null)
					_currentSchedulePeriod.PeriodTime = value;
			}
		}
    }
}