using System;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.WinCode.Settings
{
	public abstract class ScheduleRestrictionBaseView
	{
		private StartTimeLimitation _startTime;
		private EndTimeLimitation _endTime;
		private WorkTimeLimitation _workTime;
		public const double DaysPerWeek = 7;
		public const int StartDay = 1;
		protected const string DayFormat = "{0} {1}";

		/// <summary>
		/// Gets or sets target restriction information.
		/// </summary>
		protected IRestrictionBase ContainedEntity { get; private set; }

        /// <summary>
        /// Gets the current UI culture information.
        /// </summary>
        protected static CultureInfo UICulture
        {
            get { return TeleoptiPrincipal.Current.Regional.UICulture; }
        }

		/// <summary>
		/// Gets the name of the day of a week.
		/// </summary>
		public string Day { get; private set; }

		/// <summary>
		/// Gets a week number.
		/// </summary>
		public int Week { get; private set; }

		public string CreatedBy
		{
			get
			{
				IChangeInfo root = ContainedEntity.Root() as IChangeInfo;
				if (root != null && root.CreatedBy != null)
					return root.CreatedBy.Name.ToString();
				return string.Empty;
			}
		}

		public string UpdatedBy
		{
			get
			{
				IChangeInfo root = ContainedEntity.Root() as IChangeInfo;
				if (root != null && root.UpdatedBy != null)
					return root.UpdatedBy.Name.ToString();
				return string.Empty;
			}
		}

		public string CreatedOn
		{
			get
			{
				IChangeInfo root = ContainedEntity.Root() as IChangeInfo;
				if (root != null && root.CreatedOn.HasValue)
					return root.CreatedOn.Value.ToString(CultureInfo.CurrentCulture);
				return string.Empty;
			}
		}

		public string UpdatedOn
		{
			get
			{
				IChangeInfo root = ContainedEntity.Root() as IChangeInfo;
				if (root != null && root.UpdatedOn.HasValue)
					return root.UpdatedOn.Value.ToString(CultureInfo.CurrentCulture);
				return string.Empty;
			}
		}

		/// <summary>
		/// Gets or sets a minimum working time.
		/// </summary>
		/// <value>The created by.</value>
		public string MinimumWorkTime
		{
			get
			{
				return _workTime.StartTimeString;
			}
			set
			{
				_workTime = new WorkTimeLimitation(_workTime.TimeSpanFromString(value), _workTime.EndTime);
			}
		}

		/// <summary>
		/// Gets or sets a maximum working time.
		/// </summary>
		public string MaximumWorkTime
		{
			get
			{
				return _workTime.EndTimeString;
			}
			set
			{
				_workTime = new WorkTimeLimitation(_workTime.StartTime, _workTime.TimeSpanFromString(value));
			}
		}

		/// <summary>
		/// Gets or sets an earliest starting time.
		/// </summary>
		public string EarlyStartTime
		{
			get
			{
				return _startTime.StartTimeString;
			}
			set
			{
				TimeSpan timeValue;
				TimeHelper.TryParse(value, out timeValue);

				if (_endTime.EndTime.HasValue &&  timeValue > _endTime.EndTime.Value)
					throw new ArgumentOutOfRangeException(UserTexts.Resources.EarlyStartTime, "Early start time can't be greater than late end time");

				_startTime = new StartTimeLimitation(_startTime.TimeSpanFromString(value), _startTime.EndTime);
				OnStartTimeChanged();
			}
		}

		/// <summary>
		/// Gets or sets a latest start time.
		/// </summary>
		public string LateStartTime
		{
			get
			{
				return _startTime.EndTimeString;
			}
			set
			{
				_startTime = new StartTimeLimitation(_startTime.StartTime, _startTime.TimeSpanFromString(value));
				OnStartTimeChanged();
			}
		}

		/// <summary>
		/// Gets or sets an earliest end time.
		/// </summary>
		public string EarlyEndTime
		{
			get
			{
				return _endTime.StartTimeString;
			}
			set
			{
				_endTime = new EndTimeLimitation(_endTime.TimeSpanFromString(value), _endTime.EndTime);
				OnEndTimeChanged();
			}
		}

		/// <summary>
		/// Gets or sets a latest ending time.
		/// </summary>
		public string LateEndTime
		{
			get
			{
				return _endTime.EndTimeString;
			}
			set
			{

				TimeSpan timeValue;
				TimeHelper.TryParse(value, out timeValue);

				if (!string.IsNullOrEmpty(value) && _startTime.StartTime.HasValue && timeValue < _startTime.StartTime.Value)
					throw new ArgumentOutOfRangeException(UserTexts.Resources.LateEndTime, "Early start time can't be greater than late end time");

				_endTime = new EndTimeLimitation(_endTime.StartTime, _endTime.TimeSpanFromString(value));
				OnEndTimeChanged();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ScheduleRestrictionBaseView"/> class.
		/// </summary>
		/// <param name="target">A <see cref="IRestrictionBase"/> value.</param>
		/// <param name="week">A week number.</param>
		/// <param name="day">A day number between 1 and 7.</param>
		protected ScheduleRestrictionBaseView(IRestrictionBase target, int week, int day)
		{
			ContainedEntity = target;
			Day = string.Format(UICulture, DayFormat, UserTexts.Resources.Day, day);
			Week = week;
			initializeFields();
		}

		private void initializeFields()
		{
			_startTime = new StartTimeLimitation(ContainedEntity.StartTimeLimitation.StartTime, ContainedEntity.StartTimeLimitation.EndTime);
			_endTime = new EndTimeLimitation(ContainedEntity.EndTimeLimitation.StartTime, ContainedEntity.EndTimeLimitation.EndTime);
			_workTime = new WorkTimeLimitation(ContainedEntity.WorkTimeLimitation.StartTime, ContainedEntity.WorkTimeLimitation.EndTime);

		}

		protected virtual void OnStartTimeChanged() { }
		protected virtual void OnEndTimeChanged() { }

		/// <summary>
		/// Provides the starting time as <see cref="StartTimeLimitation"/> value.
		/// </summary>
		/// <returns>A <see cref="StartTimeLimitation"/> value.</returns>
		public StartTimeLimitation StartTimeLimit()
		{
			return _startTime;
		}

		/// <summary>
		/// Provides the ending time as <see cref="EndTimeLimitation"/> value.
		/// </summary>
		/// <returns>A <see cref="EndTimeLimitation"/> value.</returns>
		public EndTimeLimitation EndTimeLimit()
		{
			return _endTime;
		}

		public WorkTimeLimitation WorkTimeLimit()
		{
			return _workTime;
		}

		/// <summary>
		/// Indicates whether the specified time period is valid or not.
		/// </summary>
		/// <param name="from">A <see cref="TimeSpan?"/> reference.</param>
		/// <param name="to">A <see cref="TimeSpan?"/> reference.</param>
		/// <returns></returns>
		public static bool IsValidRange(TimeSpan? from, TimeSpan? to)
		{
			bool valid = true;
			if (from.HasValue && to.HasValue)
			{
				valid = from.Value <= to.Value;
			}
			return valid;
		}

		/// <summary>
		/// Indicates whether the specified time is an overnight time or not.
		/// </summary>
		/// <param name="time">A time reference.</param>
		/// <returns></returns>
		protected static bool IsOvernight(string time)
		{
			bool retValue = string.IsNullOrEmpty(time);

			// Splits if not null?
			if (!retValue)
			{
				retValue = time.IndexOf("+", StringComparison.CurrentCultureIgnoreCase) > -1;
			}

			return retValue;
		}

		/// <summary>
		/// Changes the specified time into an overnight time.
		/// </summary>
		/// <param name="time">A time reference.</param>
		/// <returns></returns>
		public static string ToOvernight(string time)
		{
			if (!IsOvernight(time)) time += "+1";

			return time;
		}

		/// <summary>
		/// Gets the representing week of the day.
		/// </summary>
		/// <param name="day">A day. Zero-based value.</param>
		/// <returns>A week.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "day+1")]
		public static int GetWeek(int day)
		{
			if (day == int.MinValue)
			{
				throw new ArgumentOutOfRangeException("day");
			}
			day++; // This is due to zero-based.

			int weekNumber = 1;
			if (day > DaysPerWeek)
			{
				weekNumber = (int)Math.Ceiling(day / DaysPerWeek);
			}
			return weekNumber;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static ICollection<AvailabilityRestrictionView> Parse(IAvailabilityRotation value)
		{
			ICollection<AvailabilityRestrictionView> list = new List<AvailabilityRestrictionView>();
			if (!value.AvailabilityDays.IsEmpty()) // CHECK: Why RotationDays.IsNotEmpty() not working?
			{
				int dayCount = value.DaysCount;
				int dayNumber = StartDay;
				for (int day = 0; day < dayCount; day++)
				{
					IAvailabilityDay availabilityDay = value.FindAvailabilityDay(day);
					//if (rotationDay.AvailabilityDays.Count == 0) CreateRotationRestriction(rotationDay);
					int weekNumber = GetWeek(day);

					AvailabilityRestrictionView view = new AvailabilityRestrictionView(availabilityDay.Restriction, weekNumber, dayNumber);
					list.Add(view);

					// Increases day number.
					dayNumber++;
					// Reinitializes if exceeded.
					if (dayNumber > DaysPerWeek) dayNumber = StartDay;
				}
			}
			return list;
		}

		public static ICollection<RotationRestrictionView> Parse(IRotation value)
		{
			ICollection<RotationRestrictionView> list = new List<RotationRestrictionView>();
			if (!value.RotationDays.IsEmpty())
			{
				int dayCount = value.DaysCount;
				int dayNumber = StartDay;
				for (int day = 0; day < dayCount; day++)
				{
					IRotationDay rotationDay = value.FindRotationDay(day);
					int weekNumber = GetWeek(day);
					foreach (IRotationRestriction restrictedDay in rotationDay.RestrictionCollection)
					{
						RotationRestrictionView view = new RotationRestrictionView(restrictedDay, weekNumber, dayNumber);
						list.Add(view);
					}
					// Increases day number.
					dayNumber++;
					// Reinitializes if exceeded.
					if (dayNumber > DaysPerWeek) dayNumber = StartDay;
				}
			}
			return list;
		}
	}
}
