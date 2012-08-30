﻿using System;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
    /// <summary>
    /// Schedule period child adapter.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-06-09
    /// </remarks>
    public class SchedulePeriodChildModel : GridViewModelBase<ISchedulePeriod>, 
                                                      ISchedulePeriodModel
    {
        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>The full name.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-09
        /// </remarks>
        public string FullName { get; set; }

        /// <summary>
        /// Gets the period date.
        /// </summary>
        /// <value>The period date.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-09
        /// </remarks>
        public DateOnly PeriodDate
        {
            get
            {
                return ContainedEntity.DateFrom;
            }
            set
            {
                if (value != ContainedEntity.DateFrom)
                {
                    var currentPeriod = Parent.SchedulePeriod(value);
                    if (currentPeriod != null &&
                        currentPeriod.DateFrom == value)
                    {
                        PeriodDate = value.AddDays(1);
                        return;
                    }
                    Parent.RemoveSchedulePeriod(ContainedEntity);
                    ContainedEntity.DateFrom = value;
                    Parent.AddSchedulePeriod(ContainedEntity);
                }
            }
        }

        public IPerson Parent
        {
            get { return (IPerson)ContainedEntity.Root(); }
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
                return ContainedEntity.Number;
            }

            set { ContainedEntity.Number = value; }
        }

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
				return ContainedEntity.PeriodType;
			}
			set
			{
				ContainedEntity.PeriodType = value.GetValueOrDefault();
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
        public int DaysOff
        {
            get
            {
                return ContainedEntity.GetDaysOff(ContainedEntity.DateFrom);
            }
            set { ContainedEntity.SetDaysOff(value); }
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
                TimeSpan retTimeSpan = ContainedEntity.AverageWorkTimePerDay;

                return retTimeSpan;
            }
            set
            {
                ContainedEntity.AverageWorkTimePerDay = value;
            }
        }

        /// <summary>
        /// Gets or sets the balance in.
        /// </summary>
        /// <value>The balance in.</value>
        public TimeSpan BalanceIn
        {
            get
            {
                return ContainedEntity.BalanceIn;
            }
            set
            {
                ContainedEntity.BalanceIn = value;
            }
        }

        /// <summary>
        /// Gets or sets the balance out.
        /// </summary>
        /// <value>The balance out.</value>
        public TimeSpan BalanceOut
        {
            get
            {
                return ContainedEntity.BalanceOut;
            }
            set
            {
                ContainedEntity.BalanceOut = value;
            }
        }

        /// <summary>
        /// Gets or sets the extra.
        /// </summary>
        /// <value>The extra.</value>
        public TimeSpan Extra
        {
            get
            {
                return ContainedEntity.Extra;
            }
            set
            {
                ContainedEntity.Extra = value;
            }
        }

        /// <summary>
        /// Gets or sets the seasonality.
        /// </summary>
        /// <value>The extra.</value>
        public Percent Seasonality
        {
            get
            {
                return ContainedEntity.Seasonality;
            }
            set
            {
                ContainedEntity.Seasonality = value;
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
            get { return ContainedEntity.GetDaysOff(ContainedEntity.DateFrom); }
            set { ContainedEntity.SetDaysOff(value); }
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
                return false;
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
                SchedulePeriod currentSchedulePeriod = (SchedulePeriod)ContainedEntity;
                return currentSchedulePeriod.IsDaysOffOverride;
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
                SchedulePeriod currentSchedulePeriod = (SchedulePeriod)ContainedEntity;
                return currentSchedulePeriod.IsAverageWorkTimePerDayOverride;

            }
        }
        public int MustHavePreference
        {
            get
            {
                return ContainedEntity.MustHavePreference;
            }
            set
            {
                ContainedEntity.MustHavePreference = value;
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
            get { return ContainedEntity; }
        }
    }
}