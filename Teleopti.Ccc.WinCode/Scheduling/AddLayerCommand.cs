using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public abstract class AddLayerCommand : IExecutableCommand
    {
        private readonly ISchedulerStateHolder _schedulerStateHolder;
        private readonly IScheduleViewBase _scheduleViewBase;
        private readonly ISchedulePresenterBase _presenter;
        private readonly IList<IScheduleDay> _scheduleParts;
        
        protected AddLayerCommand(ISchedulerStateHolder schedulerStateHolder, IScheduleViewBase scheduleViewBase, ISchedulePresenterBase presenter, IList<IScheduleDay> scheduleParts)
        {
            _schedulerStateHolder = schedulerStateHolder;
            _scheduleViewBase = scheduleViewBase;
            _presenter = presenter;
            _scheduleParts = scheduleParts;
        }

        public IList<IScheduleDay> ScheduleParts
        {
            get { return _scheduleParts; }
        }

        public ISchedulePresenterBase Presenter
        {
            get { return _presenter; }
        }

		public IList<IScheduleDay> SchedulePartsOnePerAgent()
		{
			var persons = new List<IPerson>();
			var filteredList = new List<IScheduleDay>();

			foreach (var day in _scheduleParts)
			{
				if (day == null || persons.Contains(day.Person)) continue;
				filteredList.Add(day);
				persons.Add(day.Person);
			}

			return filteredList;
		}

        public IScheduleViewBase ScheduleViewBase
        {
            get { return _scheduleViewBase; }
        }

        protected ISchedulerStateHolder SchedulerStateHolder
        {
            get { return _schedulerStateHolder; }
        }

        public DateTimePeriod? DefaultPeriod { get; set; }

        public abstract void Execute();

        protected bool VerifySelectedSchedule(IList<IScheduleDay> schedules)
        {
            if (schedules.Count == 0 || schedules[0] == null)
            {
                _scheduleViewBase.ShowInformationMessage(UserTexts.Resources.NoExistingShiftSelected, UserTexts.Resources.Information);
                return false;
            }
            return true;
        }

		// mark affected (+1) days to resource calculate 
		protected void MarkPeriodToBeRecalculated(DateOnlyPeriod dateOnlyPeriod)
		{

			foreach (var dateOnly in dateOnlyPeriod.DayCollection())
			{
				_schedulerStateHolder.MarkDateToBeRecalculated(dateOnly);
			}
			DateOnly dayAfter = dateOnlyPeriod.DayCollection()[dateOnlyPeriod.DayCollection().Count - 1].AddDays(1);
			_schedulerStateHolder.MarkDateToBeRecalculated(dayAfter);
		}
    }
}