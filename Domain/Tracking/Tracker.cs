using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;

namespace Teleopti.Ccc.Domain.Tracking
{
	[Serializable]
	public abstract class Tracker : ITracker
	{
		private readonly Type _type;

		private ITrackerCalculator _calculator = new TrackerCalculator();

		protected ITrackerCalculator Calculator => _calculator;

		public void InjectCalculator(ITrackerCalculator calculator)
		{
			_calculator = calculator;
		}

		protected Tracker()
		{
			_type = GetType();
		}

		public override int GetHashCode()
		{
			return _type.GetHashCode();
		}

		public static ITracker CreateDayTracker()
		{
			return new DayTracker();
		}

		public static ITracker CreateCompTracker()
		{
			return new CompTracker();
		}

		public static ITracker CreateTimeTracker()
		{
			return new TimeTracker();
		}

		public static ITracker CreateOvertimeTracker()
		{
			return new OvertimeTracker();
		}

		public static IList<ITracker> AllTrackers()
		{
			return new List<ITracker> { CreateTimeTracker(), CreateDayTracker()/*, CreateCompTracker(), CreateOvertimeTracker()*/ }.AsReadOnly();
		}

		public abstract Description Description { get; }

		public void Track(ITraceable target, IAbsence absence, IList<IScheduleDay> scheduleDays)
		{
			PerformTracking(target, absence, scheduleDays);
		}

		public abstract TimeSpan TrackForReset(IAbsence absence, IList<IScheduleDay> scheduleDays);

		public abstract IAccount CreatePersonAccount(DateOnly dateTime);

		protected abstract void PerformTracking(ITraceable target, IAbsence absence, IList<IScheduleDay> scheduleDays);
	  
		public override  bool Equals(object obj)
		{
			if (obj == null)
				return false;
			return _type == obj.GetType();
		}

		#region private classes
		private class DayTracker : Tracker
		{
			private readonly Description _description;

			internal DayTracker()
			{
				_description = new Description(UserTexts.Resources.HolidayDays);
			}

			public override Description Description => _description;

			public override TimeSpan TrackForReset(IAbsence absence, IList<IScheduleDay> scheduleDays)
			{
				return Calculator.CalculateNumberOfDaysOnScheduleDays(absence, scheduleDays);
			}

			public override IAccount CreatePersonAccount(DateOnly dateTime)
			{
				return new AccountDay(dateTime);
			}

			protected override void PerformTracking(ITraceable traceable, IAbsence absence, IList<IScheduleDay> scheduleDays)
			{
				traceable.Track(Calculator.CalculateNumberOfDaysOnScheduleDays(absence, scheduleDays));
			}

		}

		private class TimeTracker : Tracker
		{
			private readonly Description _description;

			internal TimeTracker()
			{
				_description = new Description(UserTexts.Resources.HolidayTime);
			}

			public override Description Description => _description;


			public override TimeSpan TrackForReset(IAbsence absence, IList<IScheduleDay> scheduleDays)
			{
				return Calculator.CalculateTotalTimeOnScheduleDays(absence, scheduleDays);
			}

			public override IAccount CreatePersonAccount(DateOnly dateTime)
			{
				return new AccountTime(dateTime); 
			}

			protected override void PerformTracking(ITraceable traceable, IAbsence absence, IList<IScheduleDay> scheduleDays)
			{
				traceable.Track(Calculator.CalculateTotalTimeOnScheduleDays(absence, scheduleDays));
			}
		}

		private class CompTracker : Tracker
		{
			private readonly Description _description;
			
			internal CompTracker()
			{
				_description = new Description(UserTexts.Resources.CompTime);
			}

			public override Description Description => _description;

			public override TimeSpan TrackForReset(IAbsence absence, IList<IScheduleDay> scheduleDays)
			{
				return TimeSpan.Zero;
			}

			public override IAccount CreatePersonAccount(DateOnly dateTime)
			{
				return new AccountTime(dateTime);
			}
			
			protected override void PerformTracking(ITraceable traceable, IAbsence absence, IList<IScheduleDay> scheduleDays)
			{
				//NOT IMPLEMENTED 2009-03-18
			}
		}

		private class OvertimeTracker : Tracker
		{
			private readonly Description _description;
			
			internal OvertimeTracker()
			{
				_description = new Description(UserTexts.Resources.Overtime);
			}

			public override Description Description => _description;

			public override TimeSpan TrackForReset(IAbsence absence, IList<IScheduleDay> scheduleDays)
			{
				return TimeSpan.Zero;
			}

			public override IAccount CreatePersonAccount(DateOnly dateTime)
			{
				return new AccountTime(dateTime);
			}

			protected override void PerformTracking(ITraceable target, IAbsence absence, IList<IScheduleDay> scheduleDays)
			{
				//NOT IMPLEMENTED 2009-03-18
			}

		}
		#endregion
	}
}
