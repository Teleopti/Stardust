using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings
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
            get { return TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.UICulture; }
        }

		/// <summary>
		/// Gets the name of the day of a week.
		/// </summary>
		public string Day { get; private set; }

		/// <summary>
		/// Gets a week number.
		/// </summary>
		public int Week { get; private set; }

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

		public TimeSpan? MinimumWorkTime
		{
			get
			{
				return _workTime.StartTime;
			}
			set
			{
				_workTime = new WorkTimeLimitation(value, _workTime.EndTime);
			}
		}

		public TimeSpan? MaximumWorkTime
		{
			get
			{
				return _workTime.EndTime;
			}
			set
			{
				_workTime = new WorkTimeLimitation(_workTime.StartTime, value);
			}
		}

		public TimeSpan? EarlyStartTime
		{
			get
			{
				return _startTime.StartTime;
			}
			set
			{
				if (_endTime.EndTime.HasValue && value.HasValue && value > _endTime.EndTime.Value)
					throw new ArgumentOutOfRangeException(UserTexts.Resources.EarlyStartTime, "Early start time can't be greater than late end time");

				_startTime = new StartTimeLimitation(value, _startTime.EndTime);
				OnStartTimeChanged();
			}
		}

		public TimeSpan? LateStartTime
		{
			get
			{
				return _startTime.EndTime;
			}
			set
			{
				_startTime = new StartTimeLimitation(_startTime.StartTime, value);
				OnStartTimeChanged();
			}
		}

		public TimeSpan? EarlyEndTime
		{
			get
			{
				return _endTime.StartTime;
			}
			set
			{
				_endTime = new EndTimeLimitation(value, _endTime.EndTime);
				OnEndTimeChanged();
			}
		}

		public TimeSpan? LateEndTime
		{
			get
			{
				return _endTime.EndTime;
			}
			set
			{
                if (value.HasValue && _startTime.StartTime.HasValue && value < _startTime.StartTime.Value)
					throw new ArgumentOutOfRangeException(UserTexts.Resources.LateEndTime, "Early start time can't be greater than late end time");

				_endTime = new EndTimeLimitation(_endTime.StartTime, value);
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

		public StartTimeLimitation StartTimeLimit()
		{
			return _startTime;
		}

		public EndTimeLimitation EndTimeLimit()
		{
			return _endTime;
		}

		public WorkTimeLimitation WorkTimeLimit()
		{
			return _workTime;
		}

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
