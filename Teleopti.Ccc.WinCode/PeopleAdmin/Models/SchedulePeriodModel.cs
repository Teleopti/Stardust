using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
    /// <summary>
    /// Schedule period adapter.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-06-09
    /// </remarks>
    public class SchedulePeriodModel : GridViewModelBase<ISchedulePeriod>, ISchedulePeriodModel
    {
        private readonly IPerson _containedEntity;
        private ISchedulePeriod _currentSchedulePeriod;
        private CommonNameDescriptionSetting _commonNameDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulePeriodModel"/> class.
        /// </summary>
        /// <param name="selectedDate">The selected date.</param>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-15
        /// </remarks>
        public SchedulePeriodModel(DateOnly selectedDate, IPerson person, CommonNameDescriptionSetting commonNameDescription)
        {
            _containedEntity = person;
            _currentSchedulePeriod = _containedEntity.SchedulePeriod(selectedDate);
            _commonNameDescription = commonNameDescription;

        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-09
        /// </remarks>
        public IPerson Parent
        {
            get { return _containedEntity; }
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <value>The full name.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-09
        /// </remarks>
        public string FullName
        {
            get
            {
                //return _commonNameDescription.BuildCommonNameDescription( _containedEntity);
                if (_commonNameDescription == null)
                    return _containedEntity.Name.ToString();
                else
                    return _commonNameDescription.BuildCommonNameDescription(_containedEntity);
            }
        }

        /// <summary>
        /// Gets the number.
        /// </summary>
        /// <value>The number.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-09
        /// </remarks>
        public int Number
        {
            get
            {
                if (_currentSchedulePeriod == null) return -1;
                return _currentSchedulePeriod.Number;
            }
            set { if (_currentSchedulePeriod != null) _currentSchedulePeriod.Number = value; }
        }

        /// <summary>
        /// Gets the period date.
        /// </summary>
        /// <value>The period date.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-09
        /// </remarks>
        public DateOnly? PeriodDate
        {
            get
            {
                if (_currentSchedulePeriod == null) return null;
                return _currentSchedulePeriod.DateFrom;
            }
            set
            {
                if (value != _currentSchedulePeriod.DateFrom)
                {
                    if (_currentSchedulePeriod != null)
                    {
                        if (!value.HasValue) return;
                        if (_currentSchedulePeriod != null && !_containedEntity.IsOkToAddSchedulePeriod(value.Value))
                        {
                            PeriodDate = value.Value.AddDays(1);
                            return;
                        }
                    }
                    _containedEntity.RemoveSchedulePeriod(_currentSchedulePeriod);
                    _currentSchedulePeriod.DateFrom = value.Value;
                    _containedEntity.AddSchedulePeriod(_currentSchedulePeriod);
                }
            }
        }

        /// <summary>
        /// Gets or sets the grid control.
        /// </summary>
        /// <value>The grid control.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-10
        /// </remarks>
        public GridControl GridControl { get; set; }

        /// <summary>
        /// Gets the unit.
        /// </summary>
        /// <value>The unit.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-10
        /// </remarks>
        public SchedulePeriodType? PeriodType
        {
            get
            {
                if (_currentSchedulePeriod == null) return null;
                return _currentSchedulePeriod.PeriodType;
            }
            set
            {
                if (_currentSchedulePeriod != null && value.HasValue)
                    _currentSchedulePeriod.PeriodType = value.Value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [expand state].
        /// </summary>
        /// <value><c>true</c> if [expand state]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-10
        /// </remarks>
        public bool ExpandState { get; set; }

        /// <summary>
        /// Gets the current person period by date.
        /// </summary>
        /// <param name="selectedDate">The selected date.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-15
        /// </remarks>
        public ISchedulePeriod GetCurrentPersonPeriodByDate(DateOnly selectedDate)
        {
            _currentSchedulePeriod = _containedEntity.SchedulePeriod(selectedDate);
            return _currentSchedulePeriod;
        }

        /// <summary>
        /// Gets the days off.
        /// </summary>
        /// <value>The days off.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-17
        /// </remarks>
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

        /// <summary>
        /// Gets the average work time per day.
        /// </summary>
        /// <value>The average work time per day.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-17
        /// </remarks>
        public TimeSpan AverageWorkTimePerDay
        {
            get
            {
                if (_currentSchedulePeriod == null) return TimeSpan.MinValue;
                return _currentSchedulePeriod.AverageWorkTimePerDay;
            }
        }

		/// <summary>
		/// Gets the average work time per day for display.
		/// </summary>
		/// <remarks>
		/// Created by: cs 
		/// Created date: 2008-03-10
		/// </remarks>
		public virtual TimeSpan AverageWorkTimePerDayForDisplay
		{
			get
			{
                if (_currentSchedulePeriod == null) return TimeSpan.MinValue;
				return _currentSchedulePeriod.AverageWorkTimePerDayForDisplay;
			}
			set
			{
				if (_currentSchedulePeriod != null)
					_currentSchedulePeriod.AverageWorkTimePerDayForDisplay = value;
			}
		}

        /// <summary>
        /// Balance in
        /// </summary>
        /// <value>The balance in.</value>
        /// <remarks>
        /// Created by: Tamas Balog
        /// Created date: 2010-10-28
        /// </remarks>
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

        /// <summary>
        /// Balance out
        /// </summary>
        /// <value>The balence out.</value>
        /// <remarks>
        /// Created by: Tamas Balog
        /// Created date: 2010-10-28
        /// </remarks>
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

        /// <summary>
        /// Seasonality
        /// </summary>
        /// <value>The seasonality.</value>
        /// <remarks>
        /// Created by: Tamas Balog
        /// Created date: 2011-09-22
        /// </remarks>
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

        /// <summary>
        /// Extra
        /// </summary>
        /// <value>The extra.</value>
        /// <remarks>
        /// Created by: Tamas Balog
        /// Created date: 2010-10-28
        /// </remarks>
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

        /// <summary>
        /// Gets the days off.
        /// </summary>
        /// <value>The days off.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-17
        /// </remarks>
        public int OverrideDaysOff
        {
            get
            {
                if (_currentSchedulePeriod == null)
                    return -1;
                return _currentSchedulePeriod.GetDaysOff(_currentSchedulePeriod.DateFrom);
            }
            set
            {
                if (_currentSchedulePeriod != null)
                    _currentSchedulePeriod.SetDaysOff(value);
            }
        }

        /// <summary>
        /// Gets the period count.
        /// </summary>
        /// <value>The period count.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-07-07
        /// </remarks>
        public int PeriodCount
        {
            get
            {
                if (_currentSchedulePeriod != null && Parent.PersonSchedulePeriodCollection.Count == 1)
                    return 0;
                return Parent.PersonSchedulePeriodCollection.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can gray.
        /// </summary>
        /// <value><c>true</c> if this instance can gray; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-07-23
        /// </remarks>
        public bool CanGray
        {
            get
            {
                return _currentSchedulePeriod == null;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is days off override.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is days off override; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-20
        /// </remarks>
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

        /// <summary>
        /// Gets a value indicating whether this instance is average work time per day override.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is average work time per day override; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-20
        /// </remarks>
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

        /// <summary>
        /// Gets the schedule period.
        /// </summary>
        /// <value>The schedule period.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-24
        /// </remarks>
        public ISchedulePeriod SchedulePeriod
        {
            get
            {
                return _currentSchedulePeriod;
            }
        }

        /// <summary>
        /// Resets the can bold property of child adapters.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-04
        /// </remarks>
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

		/// <summary>
		/// Gets if the is period overriden.
		/// </summary>
		/// <value>The period override value.</value>
		/// <remarks>
		/// Created by: tamasb
		/// Created date: 2012-06-15
		/// </remarks>
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

		/// <summary>
		/// Gets or sets the period time.
		/// </summary>
		/// <value>The period time.</value>
		/// <remarks>
		/// Created by: tamasb
		/// Created date: 2012-06-15
		/// </remarks>
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