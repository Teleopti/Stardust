using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
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

		public IPerson Parent => _containedEntity;

		public string FullName => _commonNameDescription == null ? _containedEntity.Name.ToString() : _commonNameDescription.BuildFor(_containedEntity);

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
	            return _currentSchedulePeriod?.DateFrom;
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
				return _currentSchedulePeriod?.PeriodType;
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

        public int? DaysOff
        {
            get
            {
				return _currentSchedulePeriod?.GetDaysOff(_currentSchedulePeriod.DateFrom);
            }
            set
            {
                if (_currentSchedulePeriod!=null && value.HasValue) 
                    _currentSchedulePeriod.SetDaysOff(value.Value);
            }
        }

        public TimeSpan AverageWorkTimePerDay => _currentSchedulePeriod?.AverageWorkTimePerDay ?? TimeSpan.MinValue;

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
				return _currentSchedulePeriod?.Extra ?? TimeSpan.MinValue;
			}
            set
            {
                if (_currentSchedulePeriod != null)
                    _currentSchedulePeriod.Extra = value;
            }
        }

        public int OverrideDaysOff
        {
            get => _currentSchedulePeriod?.DaysOff ?? -1;
			set => _currentSchedulePeriod?.SetDaysOff(value);
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

        public bool CanGray => _currentSchedulePeriod == null;

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
				SchedulePeriod currentSchedulePeriod = _currentSchedulePeriod as SchedulePeriod;
				if (currentSchedulePeriod != null)
					return currentSchedulePeriod.IsAverageWorkTimePerDayOverride;
				return false;
            }
        }
        
		public int MustHavePreference
        {
            get
			{
				return _currentSchedulePeriod?.MustHavePreference ?? 0;
			}
            set
            {
                if (_currentSchedulePeriod != null)
                {
                    _currentSchedulePeriod.MustHavePreference = value;
                }
            }
        }

        public ISchedulePeriod SchedulePeriod => _currentSchedulePeriod;

		public void ResetCanBoldPropertyOfChildAdapters()
        {
            if (GridControl != null)
            {
                var childAdapters = GridControl.Tag as
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

			var childAdapters = GridControl?.Tag as IList<SchedulePeriodChildModel>;

			if (childAdapters != null)
			{
				for (var i = 0; i < childAdapters.Count; i++)
				{
					if (childAdapters[i].CanBold) return true;
				}
			}

			return false;
		}

		public bool IsPeriodTargetOverride
		{
			get
			{
				SchedulePeriod currentSchedulePeriod = _currentSchedulePeriod as SchedulePeriod;
				if (currentSchedulePeriod != null)
					return currentSchedulePeriod.IsPeriodTimeOverride;
				return false;
			}
		}

		public TimeSpan PeriodTime
		{
			get
			{
				TimeSpan? value = _currentSchedulePeriod?.PeriodTime;
				return value ?? TimeSpan.MinValue;
			}
			set
			{
				if (_currentSchedulePeriod != null)
					_currentSchedulePeriod.PeriodTime = value;
			}
		}
    }
}